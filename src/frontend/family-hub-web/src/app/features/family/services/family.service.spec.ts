import { TestBed } from '@angular/core/testing';
import { FamilyService, Family } from './family.service';
import { GraphQLService, GraphQLError } from '../../../core/services/graphql.service';

describe('FamilyService', () => {
  let service: FamilyService;
  let graphqlService: jasmine.SpyObj<GraphQLService>;

  beforeEach(() => {
    const graphqlSpy = jasmine.createSpyObj('GraphQLService', ['query', 'mutate']);

    TestBed.configureTestingModule({
      providers: [
        FamilyService,
        { provide: GraphQLService, useValue: graphqlSpy }
      ]
    });

    service = TestBed.inject(FamilyService);
    graphqlService = TestBed.inject(GraphQLService) as jasmine.SpyObj<GraphQLService>;
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });

  describe('Initial State', () => {
    it('should initialize with null currentFamily', () => {
      expect(service.currentFamily()).toBeNull();
    });

    it('should initialize with isLoading false', () => {
      expect(service.isLoading()).toBe(false);
    });

    it('should initialize with null error', () => {
      expect(service.error()).toBeNull();
    });

    it('should initialize hasFamily computed signal to false', () => {
      expect(service.hasFamily()).toBe(false);
    });
  });

  describe('loadUserFamilies()', () => {
    it('should set isLoading to true during load', () => {
      graphqlService.query.and.returnValue(new Promise(() => {})); // Never resolves

      service.loadUserFamilies();

      expect(service.isLoading()).toBe(true);
    });

    it('should load families and set currentFamily to first family', async () => {
      const mockFamilies: Family[] = [
        {
          familyId: { value: 'family-123' },
          name: 'Smith Family',
          memberCount: 3,
          createdAt: '2025-12-30T00:00:00Z'
        }
      ];

      graphqlService.query.and.returnValue(Promise.resolve({
        getUserFamilies: {
          families: mockFamilies
        }
      }));

      await service.loadUserFamilies();

      expect(service.currentFamily()).toEqual(mockFamilies[0]);
      expect(service.isLoading()).toBe(false);
      expect(service.error()).toBeNull();
    });

    it('should handle empty families array', async () => {
      graphqlService.query.and.returnValue(Promise.resolve({
        getUserFamilies: {
          families: []
        }
      }));

      await service.loadUserFamilies();

      expect(service.currentFamily()).toBeNull();
      expect(service.isLoading()).toBe(false);
    });

    it('should set error when query fails', async () => {
      const errorMessage = 'Network error';
      graphqlService.query.and.returnValue(Promise.reject(new Error(errorMessage)));

      await service.loadUserFamilies();

      expect(service.error()).toContain(errorMessage);
      expect(service.isLoading()).toBe(false);
      expect(service.currentFamily()).toBeNull();
    });

    it('should handle GraphQLError', async () => {
      const graphqlError = new GraphQLError([
        { message: 'Unauthorized access' }
      ]);

      graphqlService.query.and.returnValue(Promise.reject(graphqlError));

      await service.loadUserFamilies();

      expect(service.error()).toContain('GraphQL errors occurred');
      expect(service.isLoading()).toBe(false);
    });

    it('should call GraphQL query with correct query string', async () => {
      graphqlService.query.and.returnValue(Promise.resolve({
        getUserFamilies: { families: [] }
      }));

      await service.loadUserFamilies();

      expect(graphqlService.query).toHaveBeenCalled();
      const callArgs = graphqlService.query.calls.mostRecent().args;
      expect(callArgs[0]).toContain('getUserFamilies');
    });
  });

  describe('createFamily()', () => {
    it('should set isLoading to true during creation', () => {
      graphqlService.mutate.and.returnValue(new Promise(() => {})); // Never resolves

      service.createFamily('Test Family');

      expect(service.isLoading()).toBe(true);
    });

    it('should create family and set currentFamily on success', async () => {
      const mockFamily: Family = {
        familyId: { value: 'new-family-456' },
        name: 'Test Family',
        memberCount: 1,
        createdAt: '2025-12-30T00:00:00Z'
      };

      graphqlService.mutate.and.returnValue(Promise.resolve({
        createFamily: {
          family: mockFamily,
          errors: null
        }
      }));

      await service.createFamily('Test Family');

      expect(service.currentFamily()).toEqual(mockFamily);
      expect(service.isLoading()).toBe(false);
      expect(service.error()).toBeNull();
    });

    it('should set error when API returns errors', async () => {
      graphqlService.mutate.and.returnValue(Promise.resolve({
        createFamily: {
          family: null,
          errors: [
            { message: 'User already has a family', code: 'BUSINESS_RULE_VIOLATION' }
          ]
        }
      }));

      await service.createFamily('Test Family');

      expect(service.error()).toBe('User already has a family');
      expect(service.currentFamily()).toBeNull();
      expect(service.isLoading()).toBe(false);
    });

    it('should handle mutation failure (network error)', async () => {
      const errorMessage = 'Failed to connect';
      graphqlService.mutate.and.returnValue(Promise.reject(new Error(errorMessage)));

      await service.createFamily('Test Family');

      expect(service.error()).toContain(errorMessage);
      expect(service.isLoading()).toBe(false);
    });

    it('should call GraphQL mutate with correct mutation string', async () => {
      graphqlService.mutate.and.returnValue(Promise.resolve({
        createFamily: { family: null, errors: null }
      }));

      await service.createFamily('Test Family');

      expect(graphqlService.mutate).toHaveBeenCalledWith(
        jasmine.stringContaining('createFamily'),
        { input: { name: 'Test Family' } }
      );
    });

    it('should pass family name as input variable', async () => {
      graphqlService.mutate.and.returnValue(Promise.resolve({
        createFamily: { family: null, errors: null }
      }));

      await service.createFamily('Smith Family');

      expect(graphqlService.mutate).toHaveBeenCalledWith(
        jasmine.anything(),
        { input: { name: 'Smith Family' } }
      );
    });
  });

  describe('hasFamily computed signal', () => {
    it('should return false when currentFamily is null', () => {
      expect(service.hasFamily()).toBe(false);
    });

    it('should return true when currentFamily is set', async () => {
      const mockFamily: Family = {
        familyId: { value: 'family-789' },
        name: 'Jones Family',
        memberCount: 2,
        createdAt: '2025-12-30T00:00:00Z'
      };

      graphqlService.query.and.returnValue(Promise.resolve({
        getUserFamilies: {
          families: [mockFamily]
        }
      }));

      await service.loadUserFamilies();

      expect(service.hasFamily()).toBe(true);
    });

    it('should reactively update when currentFamily changes', async () => {
      expect(service.hasFamily()).toBe(false);

      const mockFamily: Family = {
        familyId: { value: 'family-999' },
        name: 'New Family',
        memberCount: 1,
        createdAt: '2025-12-30T00:00:00Z'
      };

      graphqlService.mutate.and.returnValue(Promise.resolve({
        createFamily: {
          family: mockFamily,
          errors: null
        }
      }));

      await service.createFamily('New Family');

      expect(service.hasFamily()).toBe(true);
    });
  });

  describe('Error Handling', () => {
    it('should clear error before new operation', async () => {
      // First operation fails
      graphqlService.query.and.returnValue(Promise.reject(new Error('First error')));
      await service.loadUserFamilies();
      expect(service.error()).toContain('First error');

      // Second operation succeeds
      graphqlService.query.and.returnValue(Promise.resolve({
        getUserFamilies: { families: [] }
      }));
      await service.loadUserFamilies();

      expect(service.error()).toBeNull();
    });

    it('should handle unknown error types gracefully', async () => {
      graphqlService.mutate.and.returnValue(Promise.reject('String error'));

      await service.createFamily('Test');

      expect(service.error()).toBe('Failed to create family');
    });
  });

  describe('Multiple Families Scenario', () => {
    it('should set currentFamily to first family when multiple exist', async () => {
      const mockFamilies: Family[] = [
        {
          familyId: { value: 'family-1' },
          name: 'First Family',
          memberCount: 3,
          createdAt: '2025-12-30T00:00:00Z'
        },
        {
          familyId: { value: 'family-2' },
          name: 'Second Family',
          memberCount: 2,
          createdAt: '2025-12-29T00:00:00Z'
        }
      ];

      graphqlService.query.and.returnValue(Promise.resolve({
        getUserFamilies: {
          families: mockFamilies
        }
      }));

      await service.loadUserFamilies();

      expect(service.currentFamily()).toEqual(mockFamilies[0]);
      expect(service.currentFamily()?.name).toBe('First Family');
    });
  });
});
