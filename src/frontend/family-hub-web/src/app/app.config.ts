import { ApplicationConfig, provideZoneChangeDetection, APP_INITIALIZER } from '@angular/core';
import { provideRouter } from '@angular/router';
import { provideHttpClient, withInterceptors } from '@angular/common/http';
import { provideAnimations } from '@angular/platform-browser/animations';
import { provideApollo } from 'apollo-angular';
import { routes } from './app.routes';
import { authInterceptor } from './core/interceptors/auth.interceptor';
import { initializeFamily } from './core/initializers/family.initializer';
import { createApollo } from './core/graphql/apollo-config';

export const appConfig: ApplicationConfig = {
  providers: [
    provideZoneChangeDetection({ eventCoalescing: true }),
    provideRouter(routes),
    provideHttpClient(withInterceptors([authInterceptor])),
    provideAnimations(),
    // Apollo Client (GraphQL with HTTP + WebSocket subscriptions)
    provideApollo(createApollo),
    // Eager family data loading before routing
    {
      provide: APP_INITIALIZER,
      useFactory: initializeFamily,
      multi: true,
    },
  ],
};
