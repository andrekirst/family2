import { InjectionToken } from '@angular/core';
import { WidgetRegistration } from './widget-registry.model';

export const DASHBOARD_WIDGET = new InjectionToken<WidgetRegistration[]>('DASHBOARD_WIDGET');
