import { Injectable, inject } from '@angular/core';
import { Apollo } from 'apollo-angular';
import { GET_CHAIN_DEFINITIONS } from '../graphql/chain-definition.operations';
import { ChainDefinitionDto } from '../models/chain-definition.models';
import { catchError, map, of } from 'rxjs';

@Injectable({
  providedIn: 'root',
})
export class ChainDefinitionService {
  private apollo = inject(Apollo);

  getChainDefinitions(familyId: string) {
    return this.apollo
      .query<{ eventChain: { chainDefinitions: ChainDefinitionDto[] } }>({
        query: GET_CHAIN_DEFINITIONS,
        variables: { familyId },
        fetchPolicy: 'network-only',
      })
      .pipe(
        map((result) => result.data?.eventChain?.chainDefinitions ?? []),
        catchError((error) => {
          console.error('Failed to fetch chain definitions:', error);
          return of([]);
        }),
      );
  }
}
