import { Type } from '@angular/core';
import { DashboardWidgetComponent } from './dashboard-widget.interface';

export interface WidgetRegistration {
  id: string;
  title: string;
  description: string;
  component: Type<DashboardWidgetComponent>;
  defaultWidth: number;
  defaultHeight: number;
  minWidth: number;
  minHeight: number;
  maxWidth: number;
  maxHeight: number;
  requiredPermissions: string[];
}
