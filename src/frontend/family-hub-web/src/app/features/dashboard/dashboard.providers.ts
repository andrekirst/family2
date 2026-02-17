import { Provider } from '@angular/core';
import { DASHBOARD_WIDGET } from '../../core/dashboard/dashboard.tokens';
import { WidgetRegistration } from '../../core/dashboard/widget-registry.model';
import { WelcomeWidgetComponent } from './widgets/welcome-widget/welcome-widget.component';
import { FamilyOverviewWidgetComponent } from './widgets/family-overview-widget/family-overview-widget.component';
import { PendingInvitationsWidgetComponent } from './widgets/pending-invitations-widget/pending-invitations-widget.component';
import { UpcomingEventsWidgetComponent } from './widgets/upcoming-events-widget/upcoming-events-widget.component';

const DASHBOARD_WIDGETS: WidgetRegistration[] = [
  {
    id: 'dashboard:welcome',
    title: 'Welcome',
    description: 'Personalized greeting with quick action links',
    component: WelcomeWidgetComponent,
    defaultWidth: 12,
    defaultHeight: 2,
    minWidth: 6,
    minHeight: 2,
    maxWidth: 12,
    maxHeight: 4,
    requiredPermissions: [],
  },
  {
    id: 'family:overview',
    title: 'Family Overview',
    description: 'Current family status and membership info',
    component: FamilyOverviewWidgetComponent,
    defaultWidth: 6,
    defaultHeight: 3,
    minWidth: 4,
    minHeight: 2,
    maxWidth: 12,
    maxHeight: 6,
    requiredPermissions: [],
  },
  {
    id: 'family:pending-invitations',
    title: 'Pending Invitations',
    description: 'Accept or decline pending family invitations',
    component: PendingInvitationsWidgetComponent,
    defaultWidth: 6,
    defaultHeight: 3,
    minWidth: 4,
    minHeight: 2,
    maxWidth: 12,
    maxHeight: 6,
    requiredPermissions: [],
  },
  {
    id: 'family:upcoming-events',
    title: 'Upcoming Events',
    description: 'Upcoming calendar events for your family',
    component: UpcomingEventsWidgetComponent,
    defaultWidth: 6,
    defaultHeight: 3,
    minWidth: 4,
    minHeight: 2,
    maxWidth: 12,
    maxHeight: 6,
    requiredPermissions: [],
  },
];

export function provideDashboardFeature(): Provider[] {
  return [
    {
      provide: DASHBOARD_WIDGET,
      useValue: DASHBOARD_WIDGETS,
      multi: true,
    },
  ];
}
