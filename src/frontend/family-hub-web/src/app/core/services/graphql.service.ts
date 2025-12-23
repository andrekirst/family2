import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { environment } from '../../../environments/environment';
import { firstValueFrom } from 'rxjs';

@Injectable({
  providedIn: 'root'
})
export class GraphQLService {
  private readonly endpoint = environment.graphqlEndpoint;

  constructor(private http: HttpClient) {}

  async query<T>(query: string, variables?: any): Promise<T> {
    const response = await firstValueFrom(
      this.http.post<{ data: T }>(this.endpoint, { query, variables })
    );
    return response.data;
  }

  async mutate<T>(mutation: string, variables?: any): Promise<T> {
    const response = await firstValueFrom(
      this.http.post<{ data: T }>(this.endpoint, {
        query: mutation,
        variables
      })
    );
    return response.data;
  }
}
