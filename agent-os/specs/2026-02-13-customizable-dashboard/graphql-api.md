# GraphQL API — Customizable Dashboard

## Schema Overview

```graphql
# Namespace entry points (added to Root types)
type RootQuery {
  dashboard: DashboardQuery! @authorize
}

type RootMutation {
  dashboard: DashboardMutation! @authorize
}
```

---

## Queries

### availableWidgets

Returns all registered widget descriptors, filtered by user's permissions.

```graphql
type DashboardQuery {
  availableWidgets: [WidgetDescriptorDto!]!
  myDashboard: DashboardLayoutDto
  familyDashboard: DashboardLayoutDto
}

type WidgetDescriptorDto {
  widgetTypeId: String!
  module: String!
  name: String!
  description: String!
  defaultWidth: Int!
  defaultHeight: Int!
  minWidth: Int!
  minHeight: Int!
  maxWidth: Int!
  maxHeight: Int!
  configSchema: String
  requiredPermissions: [String!]!
}
```

**Handler:** Reads `IWidgetRegistry.GetAllWidgets()`, filters out widgets where user lacks `requiredPermissions`.

### myDashboard

Returns the authenticated user's personal dashboard layout with all widgets.

```graphql
type DashboardLayoutDto {
  id: UUID!
  name: String!
  isShared: Boolean!
  createdAt: DateTime!
  updatedAt: DateTime!
  widgets: [DashboardWidgetDto!]!
}

type DashboardWidgetDto {
  id: UUID!
  widgetType: String!
  x: Int!
  y: Int!
  width: Int!
  height: Int!
  sortOrder: Int!
  configJson: String
}
```

**Handler:** `IDashboardLayoutRepository.GetPersonalDashboardAsync(userId)`. Returns `null` if no dashboard exists yet (frontend generates default).

### familyDashboard

Returns the shared family dashboard. Requires user to have a family.

**Handler:** `IDashboardLayoutRepository.GetSharedDashboardAsync(familyId)`. Returns `null` if none exists.

---

## Mutations

### saveLayout

Bulk save: creates dashboard if not exists, replaces all widgets atomically.

```graphql
type DashboardMutation {
  saveLayout(input: SaveDashboardLayoutInput!): DashboardLayoutDto!
  addWidget(input: AddWidgetInput!): DashboardWidgetDto!
  removeWidget(widgetId: UUID!): Boolean!
  updateWidgetConfig(input: UpdateWidgetConfigInput!): DashboardWidgetDto!
  resetDashboard(dashboardId: UUID!): Boolean!
}

input SaveDashboardLayoutInput {
  name: String!
  isShared: Boolean!
  widgets: [WidgetPositionInput!]!
}

input WidgetPositionInput {
  id: UUID
  widgetType: String!
  x: Int!
  y: Int!
  width: Int!
  height: Int!
  sortOrder: Int!
  configJson: String
}
```

**Handler logic:**

1. Extract userId/familyId from ClaimsPrincipal
2. Get existing dashboard or create new one
3. Validate all widget types against `IWidgetRegistry.IsValidWidget()`
4. Call `DashboardLayout.ReplaceAllWidgets()`
5. Save and return updated layout

### addWidget

Adds a single widget to an existing dashboard.

```graphql
input AddWidgetInput {
  dashboardId: UUID!
  widgetType: String!
  x: Int!
  y: Int!
  width: Int!
  height: Int!
  configJson: String
}
```

**Handler logic:**

1. Validate widget type via `IWidgetRegistry.IsValidWidget()`
2. Load dashboard with widgets
3. Call `DashboardLayout.AddWidget()`
4. Save and return new widget

### removeWidget

Removes a widget from its dashboard.

**Handler logic:**

1. Find widget and its dashboard
2. Call `DashboardLayout.RemoveWidget(widgetId)`
3. Save

### updateWidgetConfig

Updates a widget's configuration JSON.

```graphql
input UpdateWidgetConfigInput {
  widgetId: UUID!
  configJson: String
}
```

**Handler logic:**

1. Find widget
2. Call `DashboardWidget.UpdateConfig(configJson)`
3. Save and return updated widget

### resetDashboard

Clears all widgets from a dashboard.

**Handler logic:**

1. Load dashboard
2. Call `DashboardLayout.ReplaceAllWidgets([])`
3. Save

---

## Example Operations (Frontend)

```typescript
// Get personal dashboard
const GET_MY_DASHBOARD = gql`
  query GetMyDashboard {
    dashboard {
      myDashboard {
        id
        name
        isShared
        createdAt
        widgets {
          id
          widgetType
          x y width height
          sortOrder
          configJson
        }
      }
    }
  }
`;

// Get available widgets for picker
const GET_AVAILABLE_WIDGETS = gql`
  query GetAvailableWidgets {
    dashboard {
      availableWidgets {
        widgetTypeId
        module name description
        defaultWidth defaultHeight
        minWidth minHeight maxWidth maxHeight
        configSchema
        requiredPermissions
      }
    }
  }
`;

// Save entire layout after editing
const SAVE_DASHBOARD_LAYOUT = gql`
  mutation SaveDashboardLayout($input: SaveDashboardLayoutInput!) {
    dashboard {
      saveLayout(input: $input) {
        id name
        widgets {
          id widgetType x y width height sortOrder configJson
        }
      }
    }
  }
`;
```
