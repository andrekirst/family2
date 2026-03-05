import { ComponentFixture, TestBed } from '@angular/core/testing';
import { ActivatedRoute, Router } from '@angular/router';
import { ReplaySubject, Subject } from 'rxjs';
import { Params } from '@angular/router';
import { signal } from '@angular/core';
import { CallbackComponent } from './callback.component';
import { AuthService } from '../../../core/auth/auth.service';
import { UserService } from '../../../core/user/user.service';

/** Flush microtasks so async subscribe callbacks complete. */
function flushPromises(): Promise<void> {
  return new Promise((r) => setTimeout(r, 0));
}

describe('CallbackComponent', () => {
  let mockRouter: { navigateByUrl: ReturnType<typeof vi.fn> };
  let mockAuthService: {
    isAuthenticated: ReturnType<typeof signal<boolean>>;
    handleCallback: ReturnType<typeof vi.fn>;
    login: ReturnType<typeof vi.fn>;
    consumePostLoginRedirect: ReturnType<typeof vi.fn>;
  };
  let mockUserService: { registerUser: ReturnType<typeof vi.fn> };

  const mockUser = {
    id: 'user-1',
    email: 'test@example.com',
    name: 'Test User',
    emailVerified: true,
    isActive: true,
    permissions: [],
  };

  beforeEach(() => {
    mockRouter = {
      navigateByUrl: vi.fn().mockResolvedValue(true),
    };

    mockAuthService = {
      isAuthenticated: signal(false),
      handleCallback: vi.fn().mockResolvedValue(undefined),
      login: vi.fn(),
      consumePostLoginRedirect: vi.fn().mockReturnValue('/dashboard'),
    };

    mockUserService = {
      registerUser: vi.fn().mockResolvedValue(mockUser),
    };
  });

  /**
   * Create a component with a Subject-based queryParams for tests
   * that need to control emission timing (e.g., take(1) test).
   */
  async function createWithSubject(): Promise<{
    component: CallbackComponent;
    fixture: ComponentFixture<CallbackComponent>;
    queryParams$: Subject<Params>;
  }> {
    const queryParams$ = new Subject<Params>();

    await TestBed.configureTestingModule({
      imports: [CallbackComponent],
      providers: [
        { provide: ActivatedRoute, useValue: { queryParams: queryParams$.asObservable() } },
        { provide: Router, useValue: mockRouter },
        { provide: AuthService, useValue: mockAuthService },
        { provide: UserService, useValue: mockUserService },
      ],
    }).compileComponents();

    const fixture = TestBed.createComponent(CallbackComponent);
    const component = fixture.componentInstance;
    fixture.detectChanges(); // trigger ngOnInit → subscribe
    return { component, fixture, queryParams$ };
  }

  /**
   * Create a component with a ReplaySubject so params are emitted
   * during ngOnInit's subscribe (same CD cycle → no NG0100).
   * Used for tests where error is set synchronously.
   */
  async function createWithParams(params: Params): Promise<{
    component: CallbackComponent;
    fixture: ComponentFixture<CallbackComponent>;
  }> {
    const queryParams$ = new ReplaySubject<Params>(1);
    queryParams$.next(params);

    await TestBed.configureTestingModule({
      imports: [CallbackComponent],
      providers: [
        { provide: ActivatedRoute, useValue: { queryParams: queryParams$.asObservable() } },
        { provide: Router, useValue: mockRouter },
        { provide: AuthService, useValue: mockAuthService },
        { provide: UserService, useValue: mockUserService },
      ],
    }).compileComponents();

    const fixture = TestBed.createComponent(CallbackComponent);
    const component = fixture.componentInstance;
    fixture.detectChanges(); // ngOnInit subscribes → ReplaySubject replays → callback fires
    await flushPromises(); // flush any async operations (handleCallback, registerUser, etc.)
    return { component, fixture };
  }

  it('should process queryParams only once (take(1))', async () => {
    const { queryParams$ } = await createWithSubject();

    queryParams$.next({ code: 'auth-code-1', state: 'state-1' });
    await flushPromises();

    queryParams$.next({ code: 'auth-code-2', state: 'state-2' });
    await flushPromises();

    expect(mockAuthService.handleCallback).toHaveBeenCalledTimes(1);
    expect(mockAuthService.handleCallback).toHaveBeenCalledWith('auth-code-1', 'state-1');
  });

  it('should redirect when already authenticated', async () => {
    mockAuthService.isAuthenticated.set(true);
    mockAuthService.consumePostLoginRedirect.mockReturnValue('/family');

    await createWithParams({ code: 'code', state: 'state' });

    expect(mockRouter.navigateByUrl).toHaveBeenCalledWith('/family');
    expect(mockAuthService.handleCallback).not.toHaveBeenCalled();
  });

  it('should show error for missing code/state', async () => {
    const { component, fixture } = await createWithParams({});

    // Error set synchronously during ngOnInit (no code/state)
    expect(component.error).toBeTruthy();
    // Verify DOM renders the error state
    fixture.detectChanges();
    expect(fixture.nativeElement.textContent).toContain('Authentication Failed');
  });

  it('should show error for OAuth error parameter', async () => {
    const { component } = await createWithParams({ error: 'access_denied' });

    expect(component.error).toContain('access_denied');
  });

  it('should complete full flow: handleCallback → registerUser → navigate', async () => {
    mockAuthService.consumePostLoginRedirect.mockReturnValue('/dashboard');

    await createWithParams({ code: 'auth-code', state: 'oauth-state' });

    expect(mockAuthService.handleCallback).toHaveBeenCalledWith('auth-code', 'oauth-state');
    expect(mockUserService.registerUser).toHaveBeenCalled();
    expect(mockRouter.navigateByUrl).toHaveBeenCalledWith('/dashboard?login=success');
  });

  it('should append &login=success when redirect has existing query params', async () => {
    mockAuthService.consumePostLoginRedirect.mockReturnValue('/dashboard?tab=overview');

    await createWithParams({ code: 'code', state: 'state' });

    expect(mockRouter.navigateByUrl).toHaveBeenCalledWith('/dashboard?tab=overview&login=success');
  });

  it('should show error when handleCallback fails', async () => {
    mockAuthService.handleCallback.mockRejectedValue(new Error('Invalid state parameter'));

    const { component } = await createWithParams({ code: 'code', state: 'bad-state' });

    expect(component.error).toBe('Invalid state parameter');
    expect(mockUserService.registerUser).not.toHaveBeenCalled();
  });

  it('should call authService.login() on retry', async () => {
    mockAuthService.handleCallback.mockRejectedValue(new Error('Token exchange failed'));

    const { component } = await createWithParams({ code: 'code', state: 'state' });

    expect(component.error).toBeTruthy();

    component.retry();

    expect(mockAuthService.login).toHaveBeenCalled();
  });
});
