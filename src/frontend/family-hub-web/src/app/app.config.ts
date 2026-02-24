import {
  APP_INITIALIZER,
  ApplicationConfig,
  LOCALE_ID,
  provideBrowserGlobalErrorListeners,
} from '@angular/core';
import { provideRouter } from '@angular/router';
import { provideHttpClient, withInterceptors } from '@angular/common/http';
import { routes } from './app.routes';
import { authInterceptor } from './core/auth/auth.interceptor';
import { provideApolloClient } from './core/graphql/apollo.config';
import { provideCalendarFeature } from './features/calendar/calendar.providers';
import { provideDashboardFeature } from './features/dashboard/dashboard.providers';
import { provideEventChainsFeature } from './features/event-chains/event-chains.providers';
import { provideFamilyFeature } from './features/family/family.providers';
import { provideFileManagementFeature } from './features/file-management/file-management.providers';
import { provideSettingsFeature } from './features/settings/settings.providers';
import { locale } from '../main';
import { provideProfileFeature } from './features/profile/profile.providers';
import { EnvironmentConfigService } from './core/config/environment-config.service';

export const appConfig: ApplicationConfig = {
  providers: [
    provideBrowserGlobalErrorListeners(),
    provideRouter(routes),
    provideHttpClient(withInterceptors([authInterceptor])),
    ...provideApolloClient(),
    ...provideCalendarFeature(),
    ...provideDashboardFeature(),
    ...provideEventChainsFeature(),
    ...provideFamilyFeature(),
    ...provideFileManagementFeature(),
    ...provideSettingsFeature(),
    ...provideProfileFeature(),
    { provide: LOCALE_ID, useValue: locale },
    {
      provide: APP_INITIALIZER,
      useFactory: (envConfig: EnvironmentConfigService) => () => envConfig.load(),
      deps: [EnvironmentConfigService],
      multi: true,
    },
  ],
};
