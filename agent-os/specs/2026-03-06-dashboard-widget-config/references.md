# References for Dashboard Widget Configuration

## Similar Implementations

### Welcome Widget (current)

- **Location**: `src/frontend/family-hub-web/src/app/features/dashboard/widgets/welcome-widget/welcome-widget.component.ts`
- **Relevance**: Base component to be modified with config-driven greeting logic
- **Key patterns**: Implements `DashboardWidgetComponent` interface, uses `widgetConfig` signal, injects `UserService`

### Upcoming Events Widget (current)

- **Location**: `src/frontend/family-hub-web/src/app/features/dashboard/widgets/upcoming-events-widget/upcoming-events-widget.component.ts`
- **Relevance**: Base component to be modified with config-driven event display
- **Key patterns**: Currently a placeholder with no data logic

### Widget Container Component

- **Location**: `src/frontend/family-hub-web/src/app/features/dashboard/components/widget-container/widget-container.component.ts`
- **Relevance**: Integration point for gear icon and settings panel
- **Key patterns**: Dynamic component loading via `ViewContainerRef`, config parsing from `configJson`, edit mode conditional UI

### Dashboard Widget Provider (backend)

- **Location**: `src/FamilyHub.Api/Features/Dashboard/DashboardWidgetProvider.cs`
- **Relevance**: Registers `dashboard:welcome` widget descriptor with `ConfigSchema: null` (to be updated)
- **Key patterns**: Implements `IWidgetProvider`, returns `WidgetDescriptor` records

### Family Widget Provider (backend)

- **Location**: `src/FamilyHub.Api/Features/Family/FamilyWidgetProvider.cs`
- **Relevance**: Registers `family:upcoming-events` widget descriptor with `ConfigSchema: null` (to be updated)
- **Key patterns**: Same `IWidgetProvider` pattern

### UpdateWidgetConfig Command (backend)

- **Location**: `src/FamilyHub.Api/Features/Dashboard/Application/Commands/UpdateWidgetConfig/`
- **Relevance**: Existing end-to-end config persistence (Command, Handler, MutationType)
- **Key patterns**: GraphQL Input -> Command pattern, repository update with `widget.UpdateConfig(configJson)`

### Widget Registry Model (frontend)

- **Location**: `src/frontend/family-hub-web/src/app/core/dashboard/widget-registry.model.ts`
- **Relevance**: `WidgetRegistration` interface to be extended with `configFields`
- **Key patterns**: Widget metadata including size constraints and permissions

### Dashboard Providers (frontend)

- **Location**: `src/frontend/family-hub-web/src/app/features/dashboard/dashboard.providers.ts`
- **Relevance**: Widget registration array to be updated with `configFields`
- **Key patterns**: Multi-provider injection token pattern for widget discovery
