# Database Schema — Customizable Dashboard

## Schema: `dashboard`

### Table: `dashboard_layouts`

| Column | Type | Constraints | Description |
|--------|------|-------------|-------------|
| `id` | `uuid` | PK, NOT NULL | Dashboard layout ID |
| `name` | `varchar(100)` | NOT NULL | Layout display name |
| `user_id` | `uuid` | NULLABLE, FK → auth.users | Owner user (personal dashboards) |
| `family_id` | `uuid` | NULLABLE, FK → family.families | Owner family (shared dashboards) |
| `is_shared` | `boolean` | NOT NULL, DEFAULT false | true = family dashboard, false = personal |
| `created_at` | `timestamptz` | NOT NULL, DEFAULT NOW() | Creation timestamp |
| `updated_at` | `timestamptz` | NOT NULL, DEFAULT NOW() | Last modification timestamp |

**Indexes:**

- `ix_dashboard_layouts_user_id` on `user_id` WHERE `user_id IS NOT NULL`
- `ix_dashboard_layouts_family_id` on `family_id` WHERE `family_id IS NOT NULL`

**Constraints:**

- CHECK: `(user_id IS NOT NULL AND family_id IS NULL AND is_shared = false) OR (user_id IS NULL AND family_id IS NOT NULL AND is_shared = true)` — enforces exclusive ownership

### Table: `dashboard_widgets`

| Column | Type | Constraints | Description |
|--------|------|-------------|-------------|
| `id` | `uuid` | PK, NOT NULL | Widget instance ID |
| `dashboard_id` | `uuid` | NOT NULL, FK → dashboard_layouts ON DELETE CASCADE | Parent dashboard |
| `widget_type` | `varchar(100)` | NOT NULL | Widget type ID (e.g., "family:overview") |
| `x` | `integer` | NOT NULL | Grid column position |
| `y` | `integer` | NOT NULL | Grid row position |
| `width` | `integer` | NOT NULL | Grid column span |
| `height` | `integer` | NOT NULL | Grid row span |
| `sort_order` | `integer` | NOT NULL, DEFAULT 0 | Ordering within dashboard |
| `config_json` | `jsonb` | NULLABLE | Widget-specific configuration |
| `created_at` | `timestamptz` | NOT NULL, DEFAULT NOW() | Creation timestamp |
| `updated_at` | `timestamptz` | NOT NULL, DEFAULT NOW() | Last modification timestamp |

**Indexes:**

- `ix_dashboard_widgets_dashboard_id` on `dashboard_id`

---

## RLS Policies

### dashboard_layouts

```sql
ALTER TABLE dashboard.dashboard_layouts ENABLE ROW LEVEL SECURITY;

CREATE POLICY dashboard_layout_policy ON dashboard.dashboard_layouts
    FOR ALL
    USING (
        ("user_id"::text = current_setting('app.current_user_id', true))
        OR
        ("family_id"::text = current_setting('app.current_family_id', true))
    );
```

**Logic:** Personal dashboards visible to owner (user_id match). Shared dashboards visible to family members (family_id match). The OR handles both cases since one column is always NULL.

### dashboard_widgets

```sql
ALTER TABLE dashboard.dashboard_widgets ENABLE ROW LEVEL SECURITY;

CREATE POLICY dashboard_widget_policy ON dashboard.dashboard_widgets
    FOR ALL
    USING ("dashboard_id" IN (
        SELECT "id" FROM dashboard.dashboard_layouts
        WHERE ("user_id"::text = current_setting('app.current_user_id', true))
           OR ("family_id"::text = current_setting('app.current_family_id', true))
    ));
```

**Logic:** Widgets inherit visibility from their parent dashboard layout via subquery.

---

## EF Core Configuration

### DashboardLayoutConfiguration

```csharp
public class DashboardLayoutConfiguration : IEntityTypeConfiguration<DashboardLayout>
{
    public void Configure(EntityTypeBuilder<DashboardLayout> builder)
    {
        builder.ToTable("dashboard_layouts", "dashboard");

        builder.HasKey(d => d.Id);
        builder.Property(d => d.Id)
            .HasConversion<DashboardId.EfCoreValueConverter>();

        builder.Property(d => d.Name)
            .HasConversion<DashboardLayoutName.EfCoreValueConverter>()
            .HasMaxLength(100).IsRequired();

        builder.Property(d => d.UserId)
            .HasConversion<UserId.EfCoreValueConverter>();

        builder.Property(d => d.FamilyId)
            .HasConversion<FamilyId.EfCoreValueConverter>();

        builder.Property(d => d.IsShared).IsRequired().HasDefaultValue(false);
        builder.Property(d => d.CreatedAt).IsRequired().HasDefaultValueSql("NOW()");
        builder.Property(d => d.UpdatedAt).IsRequired().HasDefaultValueSql("NOW()");

        builder.HasMany(d => d.Widgets)
            .WithOne()
            .HasForeignKey(w => w.DashboardId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Navigation(d => d.Widgets)
            .UsePropertyAccessMode(PropertyAccessMode.Field);

        builder.HasIndex(d => d.UserId)
            .HasFilter("\"user_id\" IS NOT NULL")
            .HasDatabaseName("ix_dashboard_layouts_user_id");

        builder.HasIndex(d => d.FamilyId)
            .HasFilter("\"family_id\" IS NOT NULL")
            .HasDatabaseName("ix_dashboard_layouts_family_id");

        builder.Ignore(d => d.DomainEvents);
    }
}
```

### DashboardWidgetConfiguration

```csharp
public class DashboardWidgetConfiguration : IEntityTypeConfiguration<DashboardWidget>
{
    public void Configure(EntityTypeBuilder<DashboardWidget> builder)
    {
        builder.ToTable("dashboard_widgets", "dashboard");

        builder.HasKey(w => w.Id);
        builder.Property(w => w.Id)
            .HasConversion<DashboardWidgetId.EfCoreValueConverter>();

        builder.Property(w => w.DashboardId)
            .HasConversion<DashboardId.EfCoreValueConverter>()
            .IsRequired();

        builder.Property(w => w.WidgetType)
            .HasConversion<WidgetTypeId.EfCoreValueConverter>()
            .HasMaxLength(100).IsRequired();

        builder.Property(w => w.X).IsRequired();
        builder.Property(w => w.Y).IsRequired();
        builder.Property(w => w.Width).IsRequired();
        builder.Property(w => w.Height).IsRequired();
        builder.Property(w => w.SortOrder).IsRequired().HasDefaultValue(0);

        builder.Property(w => w.ConfigJson)
            .HasColumnType("jsonb");

        builder.Property(w => w.CreatedAt).IsRequired().HasDefaultValueSql("NOW()");
        builder.Property(w => w.UpdatedAt).IsRequired().HasDefaultValueSql("NOW()");

        builder.HasIndex(w => w.DashboardId)
            .HasDatabaseName("ix_dashboard_widgets_dashboard_id");
    }
}
```
