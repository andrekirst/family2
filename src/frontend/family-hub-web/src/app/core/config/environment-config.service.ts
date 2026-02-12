import { Injectable } from '@angular/core';
import { environment } from '../../../environments/environment';

/**
 * Runtime environment configuration service.
 *
 * Detects whether the app is running behind the Traefik multi-environment proxy
 * (subdomain-based routing on port 3443) and derives all service URLs by convention.
 * Falls back to static environment.ts values for host-based development.
 *
 * Convention: if hostname is `{name}.localhost` on port 3443:
 *   - API:      https://api-{name}.localhost:3443/graphql
 *   - Keycloak: https://kc-{name}.localhost:3443/realms/FamilyHub
 *   - App:      https://{name}.localhost:3443
 */
@Injectable({ providedIn: 'root' })
export class EnvironmentConfigService {
  readonly apiUrl: string;
  readonly keycloak: {
    readonly issuer: string;
    readonly clientId: string;
    readonly redirectUri: string;
    readonly postLogoutRedirectUri: string;
    readonly scope: string;
  };

  constructor() {
    const hostname = window.location.hostname;
    const port = window.location.port;

    // Detect multi-env: {name}.localhost on port 3443
    const match = hostname.match(/^(.+)\.localhost$/);
    if (match && port === '3443') {
      const envName = match[1];
      const baseUrl = `https://${envName}.localhost:3443`;

      this.apiUrl = `https://api-${envName}.localhost:3443/graphql`;
      this.keycloak = {
        issuer: `https://kc-${envName}.localhost:3443/realms/FamilyHub`,
        clientId: 'familyhub-web',
        redirectUri: `${baseUrl}/callback`,
        postLogoutRedirectUri: baseUrl,
        scope: 'openid profile email',
      };
    } else {
      // Host-based development or production — use static environment values
      this.apiUrl = environment.apiUrl;
      this.keycloak = { ...environment.keycloak };
    }
  }
}
