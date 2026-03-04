import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable, catchError, of } from 'rxjs';

export interface HealthCheckDetail {
  status: string;
  description: string;
}

export interface HealthStatus {
  status: string;
  checks: Record<string, HealthCheckDetail>;
}

@Injectable({ providedIn: 'root' })
export class HealthService {
  private http = inject(HttpClient);

  checkHealth(): Observable<HealthStatus> {
    return this.http.get<HealthStatus>('/health/auth').pipe(
      catchError(() =>
        of({
          status: 'Unreachable',
          checks: {
            keycloak_oidc: { status: 'Unknown', description: 'Backend unreachable' },
            jwt_signing_keys: { status: 'Unknown', description: 'Backend unreachable' },
            graphql_schema: { status: 'Unknown', description: 'Backend unreachable' },
          },
        }),
      ),
    );
  }
}
