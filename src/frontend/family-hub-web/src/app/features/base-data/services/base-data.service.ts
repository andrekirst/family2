import { Injectable, inject } from '@angular/core';
import { Apollo } from 'apollo-angular';
import { GET_FEDERAL_STATES, GET_FEDERAL_STATE_BY_ISO3166 } from '../graphql/base-data.operations';
import { catchError, map, of } from 'rxjs';

export interface FederalStateDto {
  id: string;
  name: string;
  iso3166Code: string;
}

@Injectable({
  providedIn: 'root',
})
export class BaseDataService {
  private apollo = inject(Apollo);

  getFederalStates() {
    return this.apollo
      .query<{ baseData: { federalStates: FederalStateDto[] } }>({
        query: GET_FEDERAL_STATES,
        fetchPolicy: 'cache-first',
      })
      .pipe(
        map((result) => result.data?.baseData?.federalStates ?? []),
        catchError((error) => {
          console.error('Failed to load federal states:', error);
          return of([]);
        }),
      );
  }

  getFederalStateByIso3166(code: string) {
    return this.apollo
      .query<{ baseData: { federalStateByIso3166: FederalStateDto | null } }>({
        query: GET_FEDERAL_STATE_BY_ISO3166,
        variables: { code },
        fetchPolicy: 'cache-first',
      })
      .pipe(
        map((result) => result.data?.baseData?.federalStateByIso3166 ?? null),
        catchError((error) => {
          console.error('Failed to load federal state:', error);
          return of(null);
        }),
      );
  }
}
