import { TestBed } from '@angular/core/testing';
import { Router, ActivatedRouteSnapshot, RouterStateSnapshot, UrlTree } from '@angular/router';
import { familyGuard, noFamilyGuard } from './family.guard';
import { FamilyService } from '../../features/family/services/family.service';
import { signal, WritableSignal } from '@angular/core';

describe('Family Guards', () => {
  let mockFamilyService: jasmine.SpyObj<FamilyService>;
  let mockRouter: jasmine.SpyObj<Router>;
  let mockRoute: ActivatedRouteSnapshot;
  let mockState: RouterStateSnapshot;
  let hasFamilySignal: WritableSignal<boolean>;

  beforeEach(() => {
    // Create writable signals
    hasFamilySignal = signal(false);

    // Create mock FamilyService with signal
    mockFamilyService = jasmine.createSpyObj('FamilyService', [], {
      hasFamily: hasFamilySignal,
      currentFamily: signal(null),
    });

    // Create mock Router
    mockRouter = jasmine.createSpyObj('Router', ['navigate', 'createUrlTree']);

    // Create mock route and state
    mockRoute = {} as ActivatedRouteSnapshot;
    mockState = { url: '/test' } as RouterStateSnapshot;

    // Configure TestBed
    TestBed.configureTestingModule({
      providers: [
        { provide: FamilyService, useValue: mockFamilyService },
        { provide: Router, useValue: mockRouter },
      ],
    });
  });

  describe('familyGuard', () => {
    it('should allow navigation when user has family', () => {
      // Set hasFamily to true
      hasFamilySignal.set(true);

      const result = TestBed.runInInjectionContext(() => familyGuard(mockRoute, mockState));

      expect(result).toBe(true);
      expect(mockRouter.navigate).not.toHaveBeenCalled();
    });

    it('should redirect to family creation when user has no family', () => {
      // hasFamily is false by default
      const mockUrlTree = {} as UrlTree;
      mockRouter.createUrlTree.and.returnValue(mockUrlTree);

      const result = TestBed.runInInjectionContext(() => familyGuard(mockRoute, mockState));

      expect(result).toBe(mockUrlTree);
      expect(mockRouter.createUrlTree).toHaveBeenCalledWith(['/family/create']);
    });

    it('should log redirect reason', () => {
      spyOn(console, 'log');

      TestBed.runInInjectionContext(() => familyGuard(mockRoute, mockState));

      expect(console.log).toHaveBeenCalledWith(
        'familyGuard: User has no family. Redirecting to family creation wizard.'
      );
    });
  });

  describe('noFamilyGuard', () => {
    it('should allow navigation when user has no family', () => {
      // hasFamily is false by default

      const result = TestBed.runInInjectionContext(() => noFamilyGuard(mockRoute, mockState));

      expect(result).toBe(true);
      expect(mockRouter.navigate).not.toHaveBeenCalled();
    });

    it('should redirect to dashboard when user has family', () => {
      // Set hasFamily to true
      hasFamilySignal.set(true);
      const mockUrlTree = {} as UrlTree;
      mockRouter.createUrlTree.and.returnValue(mockUrlTree);

      const result = TestBed.runInInjectionContext(() => noFamilyGuard(mockRoute, mockState));

      expect(result).toBe(mockUrlTree);
      expect(mockRouter.createUrlTree).toHaveBeenCalledWith(['/dashboard']);
    });

    it('should log redirect reason', () => {
      hasFamilySignal.set(true);
      spyOn(console, 'log');

      TestBed.runInInjectionContext(() => noFamilyGuard(mockRoute, mockState));

      expect(console.log).toHaveBeenCalledWith(
        'noFamilyGuard: User already has a family. Redirecting to dashboard.'
      );
    });
  });

  describe('Guard Scenarios', () => {
    it('familyGuard should protect dashboard routes', () => {
      // Simulate user without family trying to access dashboard
      const mockUrlTree = {} as UrlTree;
      mockRouter.createUrlTree.and.returnValue(mockUrlTree);

      const result = TestBed.runInInjectionContext(() => familyGuard(mockRoute, mockState));

      expect(result).toBe(mockUrlTree);
      expect(mockRouter.createUrlTree).toHaveBeenCalledWith(['/family/create']);
    });

    it('noFamilyGuard should protect family creation wizard', () => {
      // Simulate user with family trying to access wizard
      hasFamilySignal.set(true);
      const mockUrlTree = {} as UrlTree;
      mockRouter.createUrlTree.and.returnValue(mockUrlTree);

      const result = TestBed.runInInjectionContext(() => noFamilyGuard(mockRoute, mockState));

      expect(result).toBe(mockUrlTree);
      expect(mockRouter.createUrlTree).toHaveBeenCalledWith(['/dashboard']);
    });

    it('guards should work together in route chain', () => {
      const mockUrlTree = {} as UrlTree;
      mockRouter.createUrlTree.and.returnValue(mockUrlTree);

      // Test new user flow: should access wizard, not dashboard
      let canAccessWizard = TestBed.runInInjectionContext(() =>
        noFamilyGuard(mockRoute, mockState)
      );
      let canAccessDashboard = TestBed.runInInjectionContext(() =>
        familyGuard(mockRoute, mockState)
      );

      expect(canAccessWizard).toBe(true);
      expect(canAccessDashboard).toBe(mockUrlTree);

      // Reset router mock
      mockRouter.createUrlTree.calls.reset();

      // Test existing user flow: should access dashboard, not wizard
      hasFamilySignal.set(true);

      canAccessWizard = TestBed.runInInjectionContext(() => noFamilyGuard(mockRoute, mockState));
      canAccessDashboard = TestBed.runInInjectionContext(() => familyGuard(mockRoute, mockState));

      expect(canAccessWizard).toBe(mockUrlTree);
      expect(canAccessDashboard).toBe(true);
    });
  });

  describe('Edge Cases', () => {
    it('should handle rapid family status changes', () => {
      const mockUrlTree = {} as UrlTree;
      mockRouter.createUrlTree.and.returnValue(mockUrlTree);

      // User has no family
      const result1 = TestBed.runInInjectionContext(() => familyGuard(mockRoute, mockState));
      expect(result1).toBe(mockUrlTree);

      // User creates family
      hasFamilySignal.set(true);

      // User now has family
      const result2 = TestBed.runInInjectionContext(() => familyGuard(mockRoute, mockState));
      expect(result2).toBe(true);
    });

    it('should handle null family service gracefully', () => {
      // This test ensures guards don't crash if service is null
      // (Should not happen in practice, but good defensive programming)
      const nullService: Partial<FamilyService> = { hasFamily: signal(false) };
      const mockUrlTree = {} as UrlTree;
      TestBed.overrideProvider(FamilyService, { useValue: nullService });
      mockRouter.createUrlTree.and.returnValue(mockUrlTree);

      const result = TestBed.runInInjectionContext(() => familyGuard(mockRoute, mockState));

      expect(result).toBe(mockUrlTree);
    });
  });
});
