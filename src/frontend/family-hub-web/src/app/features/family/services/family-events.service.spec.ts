/* eslint-disable @typescript-eslint/no-explicit-any */
import { TestBed } from '@angular/core/testing';
import { Apollo } from 'apollo-angular';
import { of, throwError, Subject } from 'rxjs';
import { FamilyEventsService } from './family-events.service';
import { ToastService } from '../../../core/services/toast.service';

describe('FamilyEventsService', () => {
  let service: FamilyEventsService;
  let apolloMock: jasmine.SpyObj<Apollo>;
  let toastServiceMock: jasmine.SpyObj<ToastService>;

  beforeEach(() => {
    // Create mock Apollo service
    apolloMock = jasmine.createSpyObj('Apollo', ['subscribe']);

    // Create mock Toast service
    toastServiceMock = jasmine.createSpyObj('ToastService', [
      'success',
      'error',
      'warning',
      'info',
    ]);

    TestBed.configureTestingModule({
      providers: [
        FamilyEventsService,
        { provide: Apollo, useValue: apolloMock },
        { provide: ToastService, useValue: toastServiceMock },
      ],
    });

    service = TestBed.inject(FamilyEventsService);
  });

  afterEach(() => {
    // Clean up subscriptions after each test
    service.unsubscribeAll();
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });

  describe('subscribeFamilyMembers', () => {
    it('should subscribe to family members changes', () => {
      const mockSubscription = of({
        data: {
          familyMembersChanged: {
            familyId: 'family-id',
            changeType: 'ADDED' as const,
            member: {
              id: 'user-id',
              email: 'test@example.com',
              role: 'MEMBER' as const,
              joinedAt: '2026-01-14T00:00:00Z',
              emailVerified: true,
              isOwner: false,
            },
          },
        },
      });

      apolloMock.subscribe.and.returnValue(mockSubscription as any);

      service.subscribeFamilyMembers('family-id');

      expect(apolloMock.subscribe).toHaveBeenCalled();
    });

    it('should update lastMemberEvent signal when event received', (done) => {
      const mockEvent = {
        familyId: 'family-id',
        changeType: 'ADDED' as const,
        member: {
          id: 'user-id',
          email: 'test@example.com',
          role: 'MEMBER' as const,
          joinedAt: '2026-01-14T00:00:00Z',
          emailVerified: true,
          isOwner: false,
        },
      };

      const mockSubscription = of({
        data: { familyMembersChanged: mockEvent },
      });

      apolloMock.subscribe.and.returnValue(mockSubscription as any);

      service.subscribeFamilyMembers('family-id');

      // Wait for async subscription to process
      setTimeout(() => {
        expect(service.lastMemberEvent()).toEqual(mockEvent);
        done();
      }, 0);
    });

    it('should show info toast when member is added', (done) => {
      const mockEvent = {
        familyId: 'family-id',
        changeType: 'ADDED' as const,
        member: {
          id: 'user-id',
          email: 'test@example.com',
          role: 'MEMBER' as const,
          joinedAt: '2026-01-14T00:00:00Z',
          emailVerified: true,
          isOwner: false,
        },
      };

      const mockSubscription = of({
        data: { familyMembersChanged: mockEvent },
      });

      apolloMock.subscribe.and.returnValue(mockSubscription as any);

      service.subscribeFamilyMembers('family-id');

      setTimeout(() => {
        expect(toastServiceMock.info).toHaveBeenCalledWith('test@example.com joined the family');
        done();
      }, 0);
    });

    it('should show info toast when member is removed', (done) => {
      const mockEvent = {
        familyId: 'family-id',
        changeType: 'REMOVED' as const,
        member: {
          id: 'user-id',
          email: 'test@example.com',
          role: 'MEMBER' as const,
          joinedAt: '2026-01-14T00:00:00Z',
          emailVerified: true,
          isOwner: false,
        },
      };

      const mockSubscription = of({
        data: { familyMembersChanged: mockEvent },
      });

      apolloMock.subscribe.and.returnValue(mockSubscription as any);

      service.subscribeFamilyMembers('family-id');

      setTimeout(() => {
        expect(toastServiceMock.info).toHaveBeenCalledWith('test@example.com left the family');
        done();
      }, 0);
    });

    it('should handle subscription errors', (done) => {
      const mockError = new Error('WebSocket connection failed');
      const mockSubscription = throwError(() => mockError);

      apolloMock.subscribe.and.returnValue(mockSubscription as any);

      service.subscribeFamilyMembers('family-id');

      setTimeout(() => {
        expect(service.isConnected()).toBe(false);
        expect(service.connectionError()).toBe('WebSocket connection failed');
        expect(toastServiceMock.error).toHaveBeenCalledWith('Lost connection to real-time updates');
        done();
      }, 0);
    });
  });

  describe('subscribePendingInvitations', () => {
    it('should subscribe to pending invitations changes', () => {
      const mockSubscription = of({
        data: {
          pendingInvitationsChanged: {
            familyId: 'family-id',
            changeType: 'ADDED' as const,
            invitation: {
              id: 'invitation-id',
              email: 'invite@example.com',
              role: 'MEMBER' as const,
              status: 'PENDING',
              invitedById: 'owner-id',
              invitedAt: '2026-01-14T00:00:00Z',
              expiresAt: '2026-01-28T00:00:00Z',
              message: 'Join us!',
              displayCode: 'ABC123',
            },
          },
        },
      });

      apolloMock.subscribe.and.returnValue(mockSubscription as any);

      service.subscribePendingInvitations('family-id');

      expect(apolloMock.subscribe).toHaveBeenCalled();
    });

    it('should update lastInvitationEvent signal when event received', (done) => {
      const mockEvent = {
        familyId: 'family-id',
        changeType: 'ADDED' as const,
        invitation: {
          id: 'invitation-id',
          email: 'invite@example.com',
          role: 'MEMBER' as const,
          status: 'PENDING',
          invitedById: 'owner-id',
          invitedAt: '2026-01-14T00:00:00Z',
          expiresAt: '2026-01-28T00:00:00Z',
          message: 'Join us!',
          displayCode: 'ABC123',
        },
      };

      const mockSubscription = of({
        data: { pendingInvitationsChanged: mockEvent },
      });

      apolloMock.subscribe.and.returnValue(mockSubscription as any);

      service.subscribePendingInvitations('family-id');

      setTimeout(() => {
        expect(service.lastInvitationEvent()).toEqual(mockEvent);
        done();
      }, 0);
    });

    it('should show info toast when invitation is added', (done) => {
      const mockEvent = {
        familyId: 'family-id',
        changeType: 'ADDED' as const,
        invitation: {
          id: 'invitation-id',
          email: 'invite@example.com',
          role: 'MEMBER' as const,
          status: 'PENDING',
          invitedById: 'owner-id',
          invitedAt: '2026-01-14T00:00:00Z',
          expiresAt: '2026-01-28T00:00:00Z',
          message: 'Join us!',
          displayCode: 'ABC123',
        },
      };

      const mockSubscription = of({
        data: { pendingInvitationsChanged: mockEvent },
      });

      apolloMock.subscribe.and.returnValue(mockSubscription as any);

      service.subscribePendingInvitations('family-id');

      setTimeout(() => {
        expect(toastServiceMock.info).toHaveBeenCalledWith('Invitation sent to invite@example.com');
        done();
      }, 0);
    });
  });

  describe('unsubscribeAll', () => {
    it('should unsubscribe from all subscriptions', () => {
       
      apolloMock.subscribe.and.returnValues(of({}) as any, of({}) as any);

      service.subscribeFamilyMembers('family-id');
      service.subscribePendingInvitations('family-id');

      service.unsubscribeAll();

      expect(service.lastMemberEvent()).toBeNull();
      expect(service.lastInvitationEvent()).toBeNull();
      expect(service.isConnected()).toBe(false);
      expect(service.connectionError()).toBeNull();
    });
  });

  describe('connection status', () => {
    it('should set isConnected to true when subscription succeeds', (done) => {
      // Use a Subject instead of `of()` to prevent immediate completion
      // Real WebSocket subscriptions stay open - they don't complete immediately
      const mockSubject = new Subject();

      apolloMock.subscribe.and.returnValue(mockSubject.asObservable() as any);

      service.subscribeFamilyMembers('family-id');

      // Emit data (simulating WebSocket message)
      mockSubject.next({
        data: {
          familyMembersChanged: {
            familyId: 'family-id',
            changeType: 'ADDED' as const,
            member: {
              id: 'user-id',
              email: 'test@example.com',
              role: 'MEMBER' as const,
              joinedAt: '2026-01-14T00:00:00Z',
              emailVerified: true,
              isOwner: false,
            },
          },
        },
      });

      setTimeout(() => {
        expect(service.isConnected()).toBe(true);
        expect(service.connectionError()).toBeNull();
        done();
      }, 0);
    });
  });
});
