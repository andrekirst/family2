import { TestBed } from '@angular/core/testing';
import { FamilyService, Family } from './family.service';
import { GraphQLService, GraphQLError } from '../../../core/services/graphql.service';
import { FamilyMember } from '../models/family.models';
import { FamilyEventsService } from './family-events.service';
import { signal } from '@angular/core';

describe('FamilyService', () => {
  let service: FamilyService;
  let graphqlService: jasmine.SpyObj<GraphQLService>;
  // Injected for DI but not directly used in current tests
  // eslint-disable-next-line @typescript-eslint/no-unused-vars
  let _familyEventsService: jasmine.SpyObj<FamilyEventsService>;

  beforeEach(() => {
    const graphqlSpy = jasmine.createSpyObj('GraphQLService', ['query', 'mutate']);
    const familyEventsSpy = jasmine.createSpyObj(
      'FamilyEventsService',
      ['subscribeFamilyMembers', 'subscribePendingInvitations', 'unsubscribeAll'],
      {
        lastMemberEvent: signal(null),
        lastInvitationEvent: signal(null),
        isConnected: signal(false),
        connectionError: signal(null),
      }
    );

    TestBed.configureTestingModule({
      providers: [
        FamilyService,
        { provide: GraphQLService, useValue: graphqlSpy },
        { provide: FamilyEventsService, useValue: familyEventsSpy },
      ],
    });

    service = TestBed.inject(FamilyService);
    graphqlService = TestBed.inject(GraphQLService) as jasmine.SpyObj<GraphQLService>;
    _familyEventsService = TestBed.inject(
      FamilyEventsService
    ) as jasmine.SpyObj<FamilyEventsService>;
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });

  describe('Initial State', () => {
    it('should initialize with null currentFamily', () => {
      expect(service.currentFamily()).toBeNull();
    });

    it('should initialize with empty familyMembers', () => {
      expect(service.familyMembers()).toEqual([]);
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

  describe('loadCurrentFamily()', () => {
    it('should set isLoading to true during load', () => {
      // eslint-disable-next-line @typescript-eslint/no-empty-function
      graphqlService.query.and.returnValue(new Promise<never>(() => {})); // Never resolves

      service.loadCurrentFamily();

      expect(service.isLoading()).toBe(true);
    });

    it('should load family and set currentFamily', async () => {
      const mockFamily: Family = {
        id: 'family-123',
        name: 'Smith Family',
        auditInfo: {
          createdAt: '2025-12-30T00:00:00Z',
          updatedAt: '2025-12-30T00:00:00Z',
        },
      };

      graphqlService.query.and.returnValue(
        Promise.resolve({
          family: mockFamily,
        })
      );

      await service.loadCurrentFamily();

      expect(service.currentFamily()).toEqual(mockFamily);
      expect(service.isLoading()).toBe(false);
      expect(service.error()).toBeNull();
      expect(service.hasFamily()).toBe(true);
    });

    it('should handle null family (user has no family)', async () => {
      graphqlService.query.and.returnValue(
        Promise.resolve({
          family: null,
        })
      );

      await service.loadCurrentFamily();

      expect(service.currentFamily()).toBeNull();
      expect(service.isLoading()).toBe(false);
      expect(service.hasFamily()).toBe(false);
    });

    it('should set error when query fails', async () => {
      const errorMessage = 'Network error';
      graphqlService.query.and.returnValue(Promise.reject(new Error(errorMessage)));

      await service.loadCurrentFamily();

      expect(service.error()).toContain(errorMessage);
      expect(service.isLoading()).toBe(false);
      expect(service.currentFamily()).toBeNull();
    });

    it('should handle GraphQLError', async () => {
      const graphqlError = new GraphQLError([{ message: 'GraphQL validation error' }]);
      graphqlService.query.and.returnValue(Promise.reject(graphqlError));

      await service.loadCurrentFamily();

      expect(service.error()).toContain('GraphQL validation error');
      expect(service.isLoading()).toBe(false);
    });

    it('should handle unknown error type with fallback message', async () => {
      graphqlService.query.and.returnValue(Promise.reject('Unknown error'));

      await service.loadCurrentFamily();

      expect(service.error()).toBe('Failed to load family');
      expect(service.isLoading()).toBe(false);
    });
  });

  describe('createFamily()', () => {
    it('should set isLoading to true during creation', () => {
      // eslint-disable-next-line @typescript-eslint/no-empty-function
      graphqlService.mutate.and.returnValue(new Promise<never>(() => {})); // Never resolves

      service.createFamily('New Family');

      expect(service.isLoading()).toBe(true);
    });

    it('should create family and set currentFamily', async () => {
      const mockFamily: Family = {
        id: 'new-family-456',
        name: 'New Family',
        auditInfo: {
          createdAt: '2025-12-30T00:00:00Z',
          updatedAt: '2025-12-30T00:00:00Z',
        },
      };

      graphqlService.mutate.and.returnValue(
        Promise.resolve({
          createFamily: {
            createdFamily: mockFamily,
            errors: null,
          },
        })
      );

      await service.createFamily('New Family');

      expect(service.currentFamily()).toEqual(mockFamily);
      expect(service.isLoading()).toBe(false);
      expect(service.error()).toBeNull();
      expect(service.hasFamily()).toBe(true);
    });

    it('should handle business logic errors', async () => {
      graphqlService.mutate.and.returnValue(
        Promise.resolve({
          createFamily: {
            family: null,
            errors: [{ message: 'User already has a family', code: 'FAMILY_ALREADY_EXISTS' }],
          },
        })
      );

      await service.createFamily('New Family');

      expect(service.currentFamily()).toBeNull();
      expect(service.error()).toBe('User already has a family');
      expect(service.isLoading()).toBe(false);
    });

    it('should set error when mutation fails', async () => {
      const errorMessage = 'Network error';
      graphqlService.mutate.and.returnValue(Promise.reject(new Error(errorMessage)));

      await service.createFamily('New Family');

      expect(service.error()).toContain(errorMessage);
      expect(service.isLoading()).toBe(false);
      expect(service.currentFamily()).toBeNull();
    });

    it('should handle empty family name validation', async () => {
      graphqlService.mutate.and.returnValue(
        Promise.resolve({
          createFamily: {
            family: null,
            errors: [{ message: 'Family name is required', code: 'VALIDATION_ERROR' }],
          },
        })
      );

      await service.createFamily('');

      expect(service.error()).toBe('Family name is required');
    });
  });

  describe('loadFamilyMembers()', () => {
    const mockFamilyId = 'family-789';

    it('should set isLoading to true during load', () => {
      // eslint-disable-next-line @typescript-eslint/no-empty-function
      graphqlService.query.and.returnValue(new Promise<never>(() => {})); // Never resolves

      service.loadFamilyMembers(mockFamilyId);

      expect(service.isLoading()).toBe(true);
    });

    it('should load family members', async () => {
      const mockMembers: FamilyMember[] = [
        {
          id: 'user-1',
          email: 'alice@example.com',
          role: 'OWNER',
          emailVerified: true,
          auditInfo: {
            createdAt: '2025-12-30T00:00:00Z',
            updatedAt: '2025-12-30T00:00:00Z',
          },
        },
        {
          id: 'user-2',
          email: 'bob@example.com',
          role: 'ADMIN',
          emailVerified: true,
          auditInfo: {
            createdAt: '2025-12-30T00:00:00Z',
            updatedAt: '2025-12-30T00:00:00Z',
          },
        },
      ];

      graphqlService.query.and.returnValue(
        Promise.resolve({
          familyMembers: mockMembers,
        })
      );

      await service.loadFamilyMembers(mockFamilyId);

      expect(service.familyMembers()).toEqual(mockMembers);
      expect(service.isLoading()).toBe(false);
      expect(service.error()).toBeNull();
    });

    it('should handle empty family members', async () => {
      graphqlService.query.and.returnValue(
        Promise.resolve({
          familyMembers: [],
        })
      );

      await service.loadFamilyMembers(mockFamilyId);

      expect(service.familyMembers()).toEqual([]);
      expect(service.isLoading()).toBe(false);
    });

    it('should set error when query fails', async () => {
      const errorMessage = 'Network error';
      graphqlService.query.and.returnValue(Promise.reject(new Error(errorMessage)));

      await service.loadFamilyMembers(mockFamilyId);

      expect(service.error()).toContain(errorMessage);
      expect(service.isLoading()).toBe(false);
      expect(service.familyMembers()).toEqual([]);
    });

    it('should handle invalid family ID', async () => {
      graphqlService.query.and.returnValue(
        Promise.resolve({
          familyMembers: [],
        })
      );

      await service.loadFamilyMembers('invalid-id');

      expect(service.familyMembers()).toEqual([]);
    });
  });

  describe('Computed Signals', () => {
    it('hasFamily should be true when currentFamily is set', async () => {
      const mockFamily: Family = {
        id: 'family-1',
        name: 'Test Family',
        auditInfo: {
          createdAt: '2025-12-30T00:00:00Z',
          updatedAt: '2025-12-30T00:00:00Z',
        },
      };

      graphqlService.query.and.returnValue(
        Promise.resolve({
          family: mockFamily,
        })
      );

      await service.loadCurrentFamily();

      expect(service.hasFamily()).toBe(true);
    });

    it('hasFamily should be false when currentFamily is null', async () => {
      graphqlService.query.and.returnValue(
        Promise.resolve({
          family: null,
        })
      );

      await service.loadCurrentFamily();

      expect(service.hasFamily()).toBe(false);
    });
  });

  describe('Error Handling', () => {
    it('should clear previous errors on new operation', async () => {
      // First operation fails
      graphqlService.query.and.returnValue(Promise.reject(new Error('First error')));
      await service.loadCurrentFamily();
      expect(service.error()).toBe('First error');

      // Second operation succeeds
      const mockFamily: Family = {
        id: 'family-999',
        name: 'Success Family',
        auditInfo: {
          createdAt: '2025-12-30T00:00:00Z',
          updatedAt: '2025-12-30T00:00:00Z',
        },
      };
      graphqlService.query.and.returnValue(
        Promise.resolve({
          family: mockFamily,
        })
      );
      await service.loadCurrentFamily();

      expect(service.error()).toBeNull();
      expect(service.currentFamily()).toEqual(mockFamily);
    });
  });
});
