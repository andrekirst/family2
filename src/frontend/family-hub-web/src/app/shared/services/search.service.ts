import { inject, Injectable, LOCALE_ID } from '@angular/core';
import { Apollo } from 'apollo-angular';
import { UNIVERSAL_SEARCH_QUERY } from '../graphql/search.operations';
import { UniversalSearchResult } from '../models/search.models';

interface UniversalSearchResponse {
  search: {
    universal: UniversalSearchResult;
  };
}

@Injectable({ providedIn: 'root' })
export class SearchService {
  private apollo = inject(Apollo);
  private localeId = inject(LOCALE_ID);

  async search(
    query: string,
    modules?: string[],
    limit?: number,
    locale?: string,
  ): Promise<UniversalSearchResult> {
    const effectiveLocale = locale ?? this.localeId;

    const result = await this.apollo
      .query<UniversalSearchResponse>({
        query: UNIVERSAL_SEARCH_QUERY,
        variables: {
          input: {
            query,
            ...(modules && { modules }),
            ...(limit && { limit }),
            ...(effectiveLocale && { locale: effectiveLocale }),
          },
        },
        fetchPolicy: 'network-only',
      })
      .toPromise();

    return (
      result?.data?.search?.universal ?? {
        results: [],
        commands: [],
      }
    );
  }
}
