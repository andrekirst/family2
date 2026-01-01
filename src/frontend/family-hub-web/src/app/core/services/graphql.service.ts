import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { environment } from '../../../environments/environment';
import { firstValueFrom } from 'rxjs';

/**
 * GraphQL error response type matching the GraphQL specification.
 * Errors can occur alongside data (partial failures) or without data.
 */
export interface GraphQLErrorResponse {
  errors?: {
    message: string;
    extensions?: {
      code?: string;
      field?: string;
      [key: string]: unknown;
    };
    [key: string]: unknown;
  }[];
}

/**
 * Custom error class for GraphQL errors.
 * Thrown when the GraphQL response contains errors.
 */
export class GraphQLError extends Error {
  constructor(public errors: GraphQLErrorResponse['errors']) {
    const messages = errors?.map(e => e.message).join(', ') || 'Unknown GraphQL error';
    super(`GraphQL errors occurred: ${messages}`);
    this.name = 'GraphQLError';

    // Maintain proper stack trace for where our error was thrown (only available on V8)
    if ('captureStackTrace' in Error) {
      (Error.captureStackTrace as (target: object, constructor: unknown) => void)(this, GraphQLError);
    }
  }
}

@Injectable({
  providedIn: 'root'
})
export class GraphQLService {
  private readonly http = inject(HttpClient);
  private readonly endpoint = environment.graphqlEndpoint;

  /**
   * Executes a GraphQL query with proper error handling.
   *
   * @param query - The GraphQL query string
   * @param variables - Optional variables for the query
   * @returns Promise resolving to the typed query result
   * @throws GraphQLError if response contains GraphQL errors
   * @throws Error if response has no data field
   */
  async query<T>(query: string, variables?: Record<string, unknown>): Promise<T> {
    const response = await firstValueFrom(
      this.http.post<{ data: T } & GraphQLErrorResponse>(
        this.endpoint,
        { query, variables }
      )
    );

    // Check for GraphQL errors first (highest priority)
    if (response?.errors && response.errors.length > 0) {
      throw new GraphQLError(response.errors);
    }

    // Check if data exists
    if (!response?.data) {
      throw new Error('No data in GraphQL response');
    }

    return response.data;
  }

  /**
   * Executes a GraphQL mutation with proper error handling.
   *
   * @param mutation - The GraphQL mutation string
   * @param variables - Optional variables for the mutation
   * @returns Promise resolving to the typed mutation result
   * @throws GraphQLError if response contains GraphQL errors
   * @throws Error if response has no data field
   */
  async mutate<T>(mutation: string, variables?: Record<string, unknown>): Promise<T> {
    const response = await firstValueFrom(
      this.http.post<{ data: T } & GraphQLErrorResponse>(
        this.endpoint,
        {
          query: mutation,
          variables
        }
      )
    );

    // Check for GraphQL errors first (highest priority)
    if (response?.errors && response.errors.length > 0) {
      throw new GraphQLError(response.errors);
    }

    // Check if data exists
    if (!response?.data) {
      throw new Error('No data in GraphQL response');
    }

    return response.data;
  }
}
