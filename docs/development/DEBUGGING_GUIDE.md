# Debugging Guide

**Purpose:** Troubleshoot common issues in Family Hub development.

**Target Audience:** Developers encountering build, runtime, or testing issues.

---

## Table of Contents

- [Build Errors](#build-errors)
- [Runtime Errors](#runtime-errors)
- [Database Issues](#database-issues)
- [RabbitMQ Issues](#rabbitmq-issues)
- [Frontend Issues](#frontend-issues)
- [Performance Profiling](#performance-profiling)
- [Vogen Value Object Issues](#vogen-value-object-issues)
- [Docker Issues](#docker-issues)

---

## Build Errors

### Backend (.NET) Build Errors

#### Error: "The type or namespace name 'Vogen' could not be found"

**Cause:** Vogen source generator not properly installed or referenced.

**Solution:**

```bash
# Restore NuGet packages
cd src/api
dotnet restore

# Clean and rebuild
dotnet clean
dotnet build

# Verify Vogen package
dotnet list package | grep Vogen
# Should show: Vogen 8.0.x
```

#### Error: "Duplicate type name 'UserId'"

**Cause:** Vogen source generator created duplicate files, or manual class conflicts with generated class.

**Solution:**

```bash
# Clean obj and bin folders
cd src/api
find . -name "obj" -type d -exec rm -rf {} +
find . -name "bin" -type d -exec rm -rf {} +

# Rebuild
dotnet build
```

**Check:** Ensure no manual `UserId.cs` exists alongside Vogen-generated one.

#### Error: "EF Core migration failed: Could not load assembly"

**Cause:** EF Core tools can't find the correct assembly.

**Solution:**

```bash
# Specify all three projects explicitly
dotnet ef migrations add MigrationName \
  --context AuthDbContext \
  --project Modules/FamilyHub.Modules.Auth \
  --startup-project FamilyHub.Api \
  --output-dir Persistence/Migrations

# Or set environment variable
export ASPNETCORE_ENVIRONMENT=Development
dotnet ef database update --context AuthDbContext --project Modules/FamilyHub.Modules.Auth --startup-project FamilyHub.Api
```

#### Error: "GraphQL schema stitching error"

**Cause:** Hot Chocolate can't merge GraphQL schemas from multiple modules.

**Solution:**

1. Check `Program.cs` GraphQL registration:

```csharp
builder.Services
    .AddGraphQLServer()
    .AddQueryType<Query>()
    .AddMutationType<Mutation>()
    .AddType<AuthMutations>()     // Ensure all module types registered
    .AddType<FamilyMutations>()
    .AddFiltering()
    .AddSorting();
```

1. Verify each module exports GraphQL types correctly.

### Frontend (Angular) Build Errors

#### Error: "Module not found: Error: Can't resolve '@angular/core'"

**Cause:** npm packages not installed or corrupted.

**Solution:**

```bash
cd src/frontend/family-hub-web

# Delete node_modules and reinstall
rm -rf node_modules package-lock.json
npm install

# Or use npm ci (cleaner)
npm ci
```

#### Error: "NG0301: Export not found!"

**Cause:** Component/service not properly exported from module or standalone component issue.

**Solution:**

1. For standalone components, ensure component is in `imports` array:

```typescript
// app.component.ts
import { SidebarComponent } from './sidebar/sidebar.component';

@Component({
  standalone: true,
  imports: [SidebarComponent],  // Add here
  // ...
})
```

1. Check component decorator has `standalone: true`:

```typescript
@Component({
  standalone: true,  // Required for standalone components
  selector: 'app-sidebar',
  // ...
})
```

#### Error: "Circular dependency detected"

**Cause:** Two components/services import each other.

**Solution:**

1. Identify cycle:

```bash
ng build --verbose 2>&1 | grep "WARNING in Circular"
```

1. Break cycle by:
   - Extracting shared code to a third file
   - Using interfaces instead of concrete classes
   - Lazy loading one of the dependencies

#### Error: "Tailwind classes not applied"

**Cause:** Tailwind not configured or purge settings too aggressive.

**Solution:**

1. Check `tailwind.config.js`:

```javascript
module.exports = {
  content: [
    "./src/**/*.{html,ts}",  // Include all HTML and TS files
  ],
  // ...
};
```

1. Rebuild:

```bash
npm run build
```

---

## Runtime Errors

### Backend Runtime Errors

#### Error: "Vogen validation failed: [Value] is invalid"

**Cause:** Invalid value passed to Vogen value object constructor.

**Solution:**

1. Check validation rules in value object:

```csharp
[ValueObject<string>]
public readonly partial struct FamilyName
{
    private static Validation Validate(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return Validation.Invalid("Family name cannot be empty.");  // This error

        if (value.Length > 100)
            return Validation.Invalid("Family name cannot exceed 100 characters.");

        return Validation.Ok;
    }
}
```

1. Fix input before creating value object:

```csharp
// Bad
var name = FamilyName.From("");  // Throws!

// Good
var input = "Smith Family".Trim();
if (!string.IsNullOrWhiteSpace(input))
{
    var name = FamilyName.From(input);
}
```

#### Error: "GraphQL error: Unexpected character"

**Cause:** GraphQL query has syntax error.

**Solution:**

1. Use GraphQL Playground to test query: `https://localhost:7000/graphql`

2. Check for:
   - Missing closing braces `}`
   - Incorrect variable syntax `$variableName`
   - Reserved keywords without escaping

3. Example fix:

```graphql
# Bad
query {
  family(id: "123") {
    name
    members {
      name  # Missing closing brace
  }
}

# Good
query {
  family(id: "123") {
    name
    members {
      name
    }
  }
}
```

#### Error: "MediatR handler not found for command"

**Cause:** Command handler not registered in DI container.

**Solution:**

1. Ensure handler class implements `IRequestHandler<TCommand, TResult>`:

```csharp
public sealed class CreateFamilyCommandHandler : IRequestHandler<CreateFamilyCommand, CreateFamilyResult>
{
    public async Task<CreateFamilyResult> Handle(CreateFamilyCommand command, CancellationToken cancellationToken)
    {
        // Implementation
    }
}
```

1. Register MediatR in `Program.cs`:

```csharp
builder.Services.AddMediatR(cfg => {
    cfg.RegisterServicesFromAssembly(typeof(CreateFamilyCommand).Assembly);
});
```

### Frontend Runtime Errors

#### Error: "Cannot read properties of undefined (reading 'subscribe')"

**Cause:** Observable not initialized before subscription.

**Solution:**

```typescript
// Bad
ngOnInit() {
  this.data$.subscribe(data => console.log(data));  // data$ is undefined
}

// Good
ngOnInit() {
  this.data$ = this.service.getData();
  this.data$.subscribe(data => console.log(data));
}

// Better (with null check)
ngOnInit() {
  this.data$?.subscribe(data => console.log(data));
}
```

#### Error: "ExpressionChangedAfterItHasBeenCheckedError"

**Cause:** Component changes value during change detection cycle.

**Solution:**

1. Defer change using `setTimeout`:

```typescript
ngAfterViewInit() {
  // Bad - changes value immediately
  this.isLoading = false;

  // Good - defers to next tick
  setTimeout(() => {
    this.isLoading = false;
  }, 0);
}
```

1. Or use `ChangeDetectorRef`:

```typescript
constructor(private cdr: ChangeDetectorRef) {}

ngAfterViewInit() {
  this.isLoading = false;
  this.cdr.detectChanges();
}
```

---

## Database Issues

### Migration Errors

#### Error: "Migration 'xxxx' has already been applied to the database"

**Cause:** Migration was already run, trying to apply again.

**Solution:**

```bash
# Check migration history
dotnet ef migrations list --context AuthDbContext --project Modules/FamilyHub.Modules.Auth --startup-project FamilyHub.Api

# If migration exists, either:
# 1. Skip it (already applied)
# 2. Remove and recreate it

# Remove migration
dotnet ef migrations remove --context AuthDbContext --project Modules/FamilyHub.Modules.Auth --startup-project FamilyHub.Api

# Recreate with new name
dotnet ef migrations add NewMigrationName --context AuthDbContext --project Modules/FamilyHub.Modules.Auth --startup-project FamilyHub.Api
```

#### Error: "Column 'xyz' does not exist"

**Cause:** Database schema out of sync with code.

**Solution:**

```bash
# Apply all pending migrations
dotnet ef database update --context AuthDbContext --project Modules/FamilyHub.Modules.Auth --startup-project FamilyHub.Api

# If still failing, check migration files
ls Modules/FamilyHub.Modules.Auth/Persistence/Migrations/

# Verify column in migration Up() method
cat Modules/FamilyHub.Modules.Auth/Persistence/Migrations/*_MigrationName.cs
```

### Connection Issues

#### Error: "Connection refused: localhost:5432"

**Cause:** PostgreSQL not running or wrong port.

**Solution:**

```bash
# Check if PostgreSQL container is running
docker-compose ps postgres

# If not running, start it
cd infrastructure/docker
docker-compose up -d postgres

# Test connection manually
docker exec -it familyhub-postgres psql -U familyhub -d familyhub

# Verify connection string in appsettings.Development.json
cat src/api/FamilyHub.Api/appsettings.Development.json | grep ConnectionStrings
```

#### Error: "Password authentication failed for user 'familyhub'"

**Cause:** Wrong password in connection string.

**Solution:**

1. Check Docker Compose password:

```bash
cat infrastructure/docker/docker-compose.yml | grep POSTGRES_PASSWORD
# Default: Dev123!
```

1. Update `appsettings.Development.json`:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Port=5432;Database=familyhub;Username=familyhub;Password=Dev123!"
  }
}
```

### RLS (Row-Level Security) Issues

#### Error: "Permission denied for table users"

**Cause:** RLS policy blocking query.

**Solution:**

1. Check RLS policies:

```sql
-- Connect to database
docker exec -it familyhub-postgres psql -U familyhub -d familyhub

-- List RLS policies
SELECT schemaname, tablename, policyname, permissive, roles, cmd, qual
FROM pg_policies
WHERE schemaname = 'auth' AND tablename = 'users';

-- Disable RLS temporarily for debugging (DEV ONLY)
ALTER TABLE auth.users DISABLE ROW LEVEL SECURITY;

-- Re-enable after debugging
ALTER TABLE auth.users ENABLE ROW LEVEL SECURITY;
```

1. Verify user context is set:

```csharp
// In repository, ensure user context is set
await _dbContext.Database.ExecuteSqlRawAsync(
    "SET LOCAL app.current_user_id = @p0",
    userId.Value
);
```

---

## RabbitMQ Issues

### Connection Issues

#### Error: "RabbitMQ.Client.Exceptions.BrokerUnreachableException"

**Cause:** RabbitMQ not running or wrong connection settings.

**Solution:**

```bash
# Check RabbitMQ container
docker-compose ps rabbitmq

# If not running
docker-compose up -d rabbitmq

# Check logs
docker-compose logs rabbitmq

# Verify connection settings in appsettings.Development.json
cat src/api/FamilyHub.Api/appsettings.Development.json | grep RabbitMQ
```

**Expected Config:**

```json
{
  "RabbitMQ": {
    "Host": "localhost",
    "Port": 5672,
    "Username": "familyhub",
    "Password": "Dev123!"
  }
}
```

#### Error: "Channel is closed"

**Cause:** RabbitMQ channel was closed unexpectedly.

**Solution:**

```csharp
// Implement retry logic
public async Task PublishEventAsync<TEvent>(TEvent @event)
{
    int retries = 3;
    while (retries > 0)
    {
        try
        {
            await _channel.BasicPublishAsync(exchange, routingKey, body);
            return;
        }
        catch (AlreadyClosedException)
        {
            retries--;
            await Task.Delay(1000);
            // Recreate channel
            _channel = _connection.CreateChannel();
        }
    }
}
```

### Message Not Received

**Debugging Steps:**

1. Check RabbitMQ Management UI: `http://localhost:15672`
   - Login: familyhub / Dev123!
   - Go to **Queues** tab
   - Verify queue exists and has messages

2. Check exchange bindings:
   - Go to **Exchanges** tab
   - Click on exchange name
   - Verify bindings to queues

3. Enable debug logging:

```csharp
// Program.cs
builder.Services.AddLogging(logging =>
{
    logging.AddConsole();
    logging.SetMinimumLevel(LogLevel.Debug);
});
```

---

## Frontend Issues

### Routing Issues

#### Error: "Cannot match any routes"

**Cause:** Route not defined or lazy loading error.

**Solution:**

1. Check `app.routes.ts`:

```typescript
export const routes: Routes = [
  { path: '', redirectTo: '/dashboard', pathMatch: 'full' },
  { path: 'dashboard', component: DashboardComponent },
  { path: 'family', component: FamilyComponent },
  { path: '**', component: NotFoundComponent },  // Catch-all
];
```

1. Verify component is standalone:

```typescript
@Component({
  standalone: true,  // Required!
  selector: 'app-dashboard',
  // ...
})
```

### HTTP Errors

#### Error: "CORS policy: No 'Access-Control-Allow-Origin' header"

**Cause:** Backend not configured for CORS.

**Solution:**

1. Check `Program.cs`:

```csharp
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        policy.WithOrigins("http://localhost:4200")
              .AllowAnyMethod()
              .AllowAnyHeader()
              .AllowCredentials();
    });
});

app.UseCors("AllowFrontend");
```

1. Ensure frontend uses correct API URL:

```typescript
// environment.ts
export const environment = {
  apiUrl: 'https://localhost:7000/graphql',  // Match backend URL
};
```

---

## Performance Profiling

### Backend Performance

#### Slow GraphQL Queries

**Tool:** Hot Chocolate tracing

```csharp
// Program.cs
builder.Services
    .AddGraphQLServer()
    .AddInstrumentation(o =>
    {
        o.IncludeDocument = true;
        o.RequestDetails = RequestDetails.All;
    });
```

**View in GraphQL Playground:**

```graphql
query {
  family(id: "123") {
    name
    members {
      name
    }
  }
}

# Check "Tracing" tab for timing details
```

#### Database Query Performance

**Use EF Core logging:**

```csharp
// appsettings.Development.json
{
  "Logging": {
    "LogLevel": {
      "Microsoft.EntityFrameworkCore.Database.Command": "Information"
    }
  }
}
```

**Output shows SQL queries with execution time.**

**Use EXPLAIN ANALYZE:**

```sql
EXPLAIN ANALYZE SELECT * FROM auth.users WHERE email = 'test@example.com';
```

### Frontend Performance

#### Slow Component Rendering

**Use Angular DevTools:**

1. Install Chrome extension: Angular DevTools
2. Open DevTools → Angular tab
3. Click "Profiler"
4. Record interaction
5. Analyze change detection cycles

**Common Issues:**

- Unnecessary change detection (use `OnPush` strategy)
- Large lists without virtual scrolling
- Heavy computations in templates (move to computed signals)

#### Bundle Size Analysis

```bash
cd src/frontend/family-hub-web

# Build with stats
ng build --stats-json

# Analyze bundle
npx webpack-bundle-analyzer dist/family-hub-web/stats.json
```

---

## Vogen Value Object Issues

### Common Errors

#### Error: "Validation failed: [Value] is invalid"

**Cause:** Input doesn't meet validation rules.

**Solution:**

1. Check validation in value object definition:

```csharp
[ValueObject<string>]
public readonly partial struct Email
{
    private static Validation Validate(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return Validation.Invalid("Email cannot be empty.");

        if (!value.Contains('@'))
            return Validation.Invalid("Invalid email format.");

        return Validation.Ok;
    }
}
```

1. Use `TryFrom` for safe creation:

```csharp
// Throws if invalid
var email = Email.From("user@example.com");

// Safe - returns false if invalid
if (Email.TryFrom("user@example.com", out var email))
{
    // Use email
}
else
{
    // Handle invalid input
}
```

#### Error: "Cannot convert from 'string' to 'FamilyName'"

**Cause:** Trying to assign string directly to Vogen type.

**Solution:**

```csharp
// Bad
FamilyName name = "Smith Family";  // Compile error

// Good
FamilyName name = FamilyName.From("Smith Family");

// Or for primitives
string nameString = name.Value;  // Extract underlying string
```

#### Error: "EF Core cannot map Vogen value object"

**Cause:** Missing EF Core value converter.

**Solution:**

```csharp
// Value object definition - include converter
[ValueObject<Guid>(conversions: Conversions.EfCoreValueConverter)]
public readonly partial struct UserId { }

// Entity configuration
builder.Property(u => u.Id)
    .HasConversion(new UserId.EfCoreValueConverter())
    .IsRequired();
```

---

## Docker Issues

### Container Won't Start

**Debugging Steps:**

```bash
# Check container status
docker-compose ps

# View logs
docker-compose logs <service-name>
docker-compose logs postgres
docker-compose logs rabbitmq

# Restart service
docker-compose restart <service-name>

# Rebuild service (if Dockerfile changed)
docker-compose up -d --build <service-name>

# Remove and recreate
docker-compose down
docker-compose up -d
```

### Port Conflicts

**Error:** "port is already allocated"

**Solution:**

```bash
# Find process using port
lsof -i :5432  # PostgreSQL
lsof -i :5672  # RabbitMQ
lsof -i :8080  # Zitadel

# Kill process
kill -9 <PID>

# Or change port in docker-compose.yml
ports:
  - "5433:5432"  # Map to different host port
```

### Volume Issues

**Error:** "Database initialization failed"

**Solution:**

```bash
# Stop containers
docker-compose down

# Remove volumes (WARNING: deletes all data)
docker-compose down -v

# Restart
docker-compose up -d
```

---

## Quick Debugging Checklist

When encountering an issue:

1. ✅ **Check logs:**
   - Backend: Console output or `dotnet run`
   - Frontend: Browser console (F12)
   - Docker: `docker-compose logs <service>`

2. ✅ **Verify services running:**
   - `docker-compose ps` - All services "Up (healthy)"
   - Backend: `curl https://localhost:7000/graphql -k`
   - Frontend: Open `http://localhost:4200`

3. ✅ **Check configuration:**
   - Connection strings match Docker Compose
   - API URLs match backend
   - OAuth settings correct

4. ✅ **Clear caches:**
   - Backend: Delete `bin/` and `obj/` folders
   - Frontend: Delete `node_modules/` and reinstall
   - Docker: `docker-compose down -v`

5. ✅ **Search documentation:**
   - [LOCAL_DEVELOPMENT_SETUP.md](LOCAL_DEVELOPMENT_SETUP.md#common-issues--troubleshooting)
   - [WORKFLOWS.md](WORKFLOWS.md)
   - This guide

---

## Getting Help

If issue persists after trying these solutions:

1. **Create GitHub Issue:** Use `.github/ISSUE_TEMPLATE/bug_report.md`
2. **Include:**
   - Steps to reproduce
   - Expected vs actual behavior
   - Logs (backend + frontend + Docker)
   - Environment (OS, Node version, .NET version)
3. **Search Existing Issues:** Check if already reported

---

**Last Updated:** 2026-01-09
**Version:** 1.0.0
