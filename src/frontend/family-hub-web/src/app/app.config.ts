import { ApplicationConfig, provideBrowserGlobalErrorListeners } from '@angular/core';
import { provideRouter } from '@angular/router';
import { provideHttpClient, withInterceptors } from '@angular/common/http';
import { routes } from './app.routes';
import { authInterceptor } from './core/auth/auth.interceptor';
import { provideApolloClient } from './core/graphql/apollo.config';
import { provideCalendarFeature } from './features/calendar/calendar.providers';
import { provideEventChainsFeature } from './features/event-chains/event-chains.providers';
import { provideFamilyFeature } from './features/family/family.providers';
import { provideSettingsFeature } from './features/settings/settings.providers';

export const appConfig: ApplicationConfig = {
  providers: [
    provideBrowserGlobalErrorListeners(),
    provideRouter(routes),
    provideHttpClient(withInterceptors([authInterceptor])),
    ...provideApolloClient(),
    ...provideCalendarFeature(),
    ...provideEventChainsFeature(),
    ...provideFamilyFeature(),
    ...provideSettingsFeature(),
  ],
};
