import { TestBed } from '@angular/core/testing';
import { HttpClientTestingModule, HttpTestingController } from '@angular/common/http/testing';
import { GraphQLService, GraphQLError } from './graphql.service';
import { environment } from '../../../environments/environment';

describe('GraphQLService', () => {
  let service: GraphQLService;
  let httpMock: HttpTestingController;

  beforeEach(() => {
    TestBed.configureTestingModule({
      imports: [HttpClientTestingModule],
      providers: [GraphQLService]
    });
    service = TestBed.inject(GraphQLService);
    httpMock = TestBed.inject(HttpTestingController);
  });

  afterEach(() => {
    httpMock.verify();
  });

  describe('query', () => {
    it('should return data on successful query', async () => {
      const mockData = { users: [{ id: '1', name: 'Test User' }] };
      const query = 'query { users { id name } }';

      const queryPromise = service.query<typeof mockData>(query);

      const req = httpMock.expectOne(environment.graphqlEndpoint);
      expect(req.request.method).toBe('POST');
      expect(req.request.body.query).toBe(query);

      req.flush({ data: mockData });

      const result = await queryPromise;
      expect(result).toEqual(mockData);
    });

    it('should pass variables to query', async () => {
      const mockData = { user: { id: '1', name: 'Test User' } };
      const query = 'query GetUser($id: ID!) { user(id: $id) { id name } }';
      const variables = { id: '1' };

      const queryPromise = service.query<typeof mockData>(query, variables);

      const req = httpMock.expectOne(environment.graphqlEndpoint);
      expect(req.request.body.variables).toEqual(variables);

      req.flush({ data: mockData });

      await queryPromise;
    });

    it('should throw GraphQLError when response contains errors', async () => {
      const query = 'query { invalidQuery }';
      const mockErrors = [
        {
          message: 'Field "invalidQuery" doesn\'t exist on type "Query"',
          extensions: {
            code: 'GRAPHQL_VALIDATION_FAILED',
            field: 'invalidQuery'
          }
        }
      ];

      const queryPromise = service.query(query);

      const req = httpMock.expectOne(environment.graphqlEndpoint);
      req.flush({ errors: mockErrors, data: null });

      await expectAsync(queryPromise).toBeRejectedWithError(GraphQLError);

      try {
        await queryPromise;
      } catch (error) {
        expect(error).toBeInstanceOf(GraphQLError);
        const graphqlError = error as GraphQLError;
        expect(graphqlError.errors).toEqual(mockErrors);
        expect(graphqlError.message).toContain('GraphQL errors occurred');
      }
    });

    it('should throw error when response has no data field', async () => {
      const query = 'query { users }';

      const queryPromise = service.query(query);

      const req = httpMock.expectOne(environment.graphqlEndpoint);
      req.flush({});

      await expectAsync(queryPromise).toBeRejectedWithError('No data in GraphQL response');
    });

    it('should throw error when data is null and no errors present', async () => {
      const query = 'query { users }';

      const queryPromise = service.query(query);

      const req = httpMock.expectOne(environment.graphqlEndpoint);
      req.flush({ data: null });

      await expectAsync(queryPromise).toBeRejectedWithError('No data in GraphQL response');
    });

    it('should handle network errors', async () => {
      const query = 'query { users }';

      const queryPromise = service.query(query);

      const req = httpMock.expectOne(environment.graphqlEndpoint);
      req.error(new ProgressEvent('Network error'), {
        status: 0,
        statusText: 'Unknown Error'
      });

      await expectAsync(queryPromise).toBeRejected();
    });

    it('should handle HTTP 500 errors', async () => {
      const query = 'query { users }';

      const queryPromise = service.query(query);

      const req = httpMock.expectOne(environment.graphqlEndpoint);
      req.flush('Internal Server Error', {
        status: 500,
        statusText: 'Internal Server Error'
      });

      await expectAsync(queryPromise).toBeRejected();
    });
  });

  describe('mutate', () => {
    it('should return data on successful mutation', async () => {
      const mockData = { createUser: { id: '1', name: 'New User' } };
      const mutation = 'mutation { createUser(name: "New User") { id name } }';

      const mutatePromise = service.mutate<typeof mockData>(mutation);

      const req = httpMock.expectOne(environment.graphqlEndpoint);
      expect(req.request.method).toBe('POST');
      expect(req.request.body.query).toBe(mutation);

      req.flush({ data: mockData });

      const result = await mutatePromise;
      expect(result).toEqual(mockData);
    });

    it('should pass variables to mutation', async () => {
      const mockData = { createFamily: { familyId: '1', name: 'Smith Family' } };
      const mutation = 'mutation CreateFamily($input: CreateFamilyInput!) { createFamily(input: $input) { familyId name } }';
      const variables = { input: { name: 'Smith Family' } };

      const mutatePromise = service.mutate<typeof mockData>(mutation, variables);

      const req = httpMock.expectOne(environment.graphqlEndpoint);
      expect(req.request.body.variables).toEqual(variables);

      req.flush({ data: mockData });

      await mutatePromise;
    });

    it('should throw GraphQLError when mutation returns errors', async () => {
      const mutation = 'mutation { createFamily(input: {}) }';
      const mockErrors = [
        {
          message: 'User already has a family',
          extensions: {
            code: 'BUSINESS_RULE_VIOLATION'
          }
        }
      ];

      const mutatePromise = service.mutate(mutation);

      const req = httpMock.expectOne(environment.graphqlEndpoint);
      req.flush({ errors: mockErrors, data: null });

      await expectAsync(mutatePromise).toBeRejectedWithError(GraphQLError);

      try {
        await mutatePromise;
      } catch (error) {
        expect(error).toBeInstanceOf(GraphQLError);
        const graphqlError = error as GraphQLError;
        expect(graphqlError.errors).toEqual(mockErrors);
      }
    });

    it('should throw error when mutation response has no data', async () => {
      const mutation = 'mutation { createUser }';

      const mutatePromise = service.mutate(mutation);

      const req = httpMock.expectOne(environment.graphqlEndpoint);
      req.flush({});

      await expectAsync(mutatePromise).toBeRejectedWithError('No data in GraphQL response');
    });

    it('should handle partial data with errors (graceful degradation)', async () => {
      const mutation = 'mutation { batchCreate }';
      const mockData = { batchCreate: { successCount: 5, failedCount: 2 } };
      const mockErrors = [
        {
          message: 'Some items failed to create',
          extensions: { code: 'PARTIAL_FAILURE' }
        }
      ];

      const mutatePromise = service.mutate(mutation);

      const req = httpMock.expectOne(environment.graphqlEndpoint);
      req.flush({ data: mockData, errors: mockErrors });

      // Should throw error even with partial data, as errors take precedence
      await expectAsync(mutatePromise).toBeRejectedWithError(GraphQLError);
    });
  });

  describe('GraphQLError', () => {
    it('should create error with proper message and errors array', () => {
      const errors = [
        { message: 'Error 1', extensions: { code: 'CODE_1' } },
        { message: 'Error 2', extensions: { code: 'CODE_2' } }
      ];

      const graphqlError = new GraphQLError(errors);

      expect(graphqlError.message).toBe('GraphQL errors occurred: Error 1, Error 2');
      expect(graphqlError.errors).toEqual(errors);
      expect(graphqlError.name).toBe('GraphQLError');
    });

    it('should handle single error', () => {
      const errors = [{ message: 'Single error' }];

      const graphqlError = new GraphQLError(errors);

      expect(graphqlError.message).toBe('GraphQL errors occurred: Single error');
    });

    it('should handle errors without extensions', () => {
      const errors = [{ message: 'Simple error' }];

      const graphqlError = new GraphQLError(errors);

      expect(graphqlError.errors?.[0]?.extensions).toBeUndefined();
    });
  });
});
