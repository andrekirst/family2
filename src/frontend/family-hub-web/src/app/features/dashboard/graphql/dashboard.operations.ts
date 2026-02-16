import { gql } from 'apollo-angular';

export const GET_MY_DASHBOARD = gql`
  query GetMyDashboard {
    dashboard {
      myDashboard {
        id
        name
        isShared
        createdAt
        updatedAt
        widgets {
          id
          widgetType
          x
          y
          width
          height
          sortOrder
          configJson
        }
      }
    }
  }
`;

export const GET_FAMILY_DASHBOARD = gql`
  query GetFamilyDashboard {
    dashboard {
      familyDashboard {
        id
        name
        isShared
        createdAt
        updatedAt
        widgets {
          id
          widgetType
          x
          y
          width
          height
          sortOrder
          configJson
        }
      }
    }
  }
`;

export const GET_AVAILABLE_WIDGETS = gql`
  query GetAvailableWidgets {
    dashboard {
      availableWidgets {
        widgetTypeId
        module
        name
        description
        defaultWidth
        defaultHeight
        minWidth
        minHeight
        maxWidth
        maxHeight
        configSchema
        requiredPermissions
      }
    }
  }
`;

export const SAVE_DASHBOARD_LAYOUT = gql`
  mutation SaveDashboardLayout($input: SaveDashboardLayoutInput!) {
    dashboard {
      saveLayout(input: $input) {
        id
        name
        isShared
        createdAt
        updatedAt
        widgets {
          id
          widgetType
          x
          y
          width
          height
          sortOrder
          configJson
        }
      }
    }
  }
`;

export const ADD_WIDGET = gql`
  mutation AddWidget($input: AddWidgetInput!) {
    dashboard {
      addWidget(input: $input) {
        id
        widgetType
        x
        y
        width
        height
        sortOrder
        configJson
      }
    }
  }
`;

export const REMOVE_WIDGET = gql`
  mutation RemoveWidget($widgetId: UUID!) {
    dashboard {
      removeWidget(widgetId: $widgetId)
    }
  }
`;

export const RESET_DASHBOARD = gql`
  mutation ResetDashboard($dashboardId: UUID!) {
    dashboard {
      resetDashboard(dashboardId: $dashboardId)
    }
  }
`;

export interface DashboardWidgetDto {
  id: string;
  widgetType: string;
  x: number;
  y: number;
  width: number;
  height: number;
  sortOrder: number;
  configJson: string | null;
}

export interface DashboardLayoutDto {
  id: string;
  name: string;
  isShared: boolean;
  createdAt: string;
  updatedAt: string;
  widgets: DashboardWidgetDto[];
}

export interface WidgetDescriptorDto {
  widgetTypeId: string;
  module: string;
  name: string;
  description: string;
  defaultWidth: number;
  defaultHeight: number;
  minWidth: number;
  minHeight: number;
  maxWidth: number;
  maxHeight: number;
  configSchema: string | null;
  requiredPermissions: string[];
}
