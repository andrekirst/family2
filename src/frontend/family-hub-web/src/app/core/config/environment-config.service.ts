import { Injectable } from '@angular/core';
import { environment } from '../../../environments/environment';

interface FrontendConfig {
  apiUrl: string;
  keycloak: {
    issuer: string;
    clientId: string;
    redirectUri: string;
    postLogoutRedirectUri: string;
    scope: string;
  };
}

/**
 * Runtime environment configuration service.
 *
 * Loads configuration at app startup via three strategies (in order):
 *   1. Fetch `/config` from same-origin (proxied by Traefik to API in Docker environments)
 *   2. Fall back to hostname-based detection for multi-env setups
 *   3. Fall back to static environment.ts values for host-based development
 *
 * The `load()` method is called by APP_INITIALIZER before Angular bootstrap completes,
 * ensuring all components have access to correct configuration from the start.
 */
@Injectable({ providedIn: 'root' })
export class EnvironmentConfigService {
  private _config!: FrontendConfig;

  get apiUrl(): string {
    return this._config.apiUrl;
  }

  get apiBaseUrl(): string {
    return this._config.apiUrl.replace(/\/graphql$/, '');
  }

  get keycloak(): FrontendConfig['keycloak'] {
    return this._config.keycloak;
  }

  /**
   * Load configuration. Called once during APP_INITIALIZER.
   * Uses native fetch() because Angular's HttpClient is not yet available.
   */
  async load(): Promise<void> {
    // Strategy 1: Fetch /config from same-origin (works in Docker environments)
    try {
      const response = await fetch('/config', {
        signal: AbortSignal.timeout(3000),
      });
      if (response.ok) {
        const config = await response.json();
        if (config?.apiUrl && config?.keycloak?.issuer) {
          this._config = config;
          return;
        }
      }
    } catch {
      // /config not available — fall through to hostname detection
    }

    // Strategy 2: Hostname-based detection for multi-env proxy setups
    const hostname = window.location.hostname;
    const port = window.location.port;
    const match = hostname.match(/^(.+)\.(localhost|dev\.andrekirst\.de)$/);

    if (match && port === '4443') {
      const envName = match[1];
      const domain = match[2];
      const baseUrl = `https://${envName}.${domain}:4443`;

      this._config = {
        apiUrl: `${baseUrl}/graphql`,
        keycloak: {
          issuer: `https://auth.${domain}:4443/realms/FamilyHub-${envName}`,
          clientId: 'familyhub-web',
          redirectUri: `${baseUrl}/callback`,
          postLogoutRedirectUri: baseUrl,
          scope: 'openid profile email',
        },
      };
      return;
    }

    // Strategy 3: Static environment.ts values (host-based dev or production)
    this._config = {
      apiUrl: environment.apiUrl,
      keycloak: { ...environment.keycloak },
    };
  }
}
