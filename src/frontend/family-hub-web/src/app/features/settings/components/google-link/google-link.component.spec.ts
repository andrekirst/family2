import { ComponentFixture, TestBed } from '@angular/core/testing';
import { signal, computed } from '@angular/core';
import { GoogleLinkComponent } from './google-link.component';
import { GoogleIntegrationService } from '../../services/google-integration.service';
import { LinkedAccount } from '../../models/google-integration.models';

describe('GoogleLinkComponent', () => {
  let component: GoogleLinkComponent;
  let fixture: ComponentFixture<GoogleLinkComponent>;
  let nativeElement: HTMLElement;
  let mockService: Partial<GoogleIntegrationService>;

  const mockAccount: LinkedAccount = {
    googleAccountId: 'google-123',
    googleEmail: 'test@gmail.com',
    status: 'Active',
    grantedScopes: 'openid email',
    lastSyncAt: null,
    createdAt: '2026-02-01T00:00:00Z',
  };

  beforeEach(async () => {
    const linkedAccounts = signal<LinkedAccount[]>([]);

    mockService = {
      linkedAccounts,
      syncStatus: signal(null),
      loading: signal(false),
      error: signal<string | null>(null),
      isLinked: computed(() => linkedAccounts().length > 0),
      primaryAccount: computed(() => linkedAccounts()[0] ?? null),
      loadLinkedAccounts: vi.fn(),
      linkGoogle: vi.fn(),
      unlinkGoogle: vi.fn(),
      refreshToken: vi.fn(),
    };

    await TestBed.configureTestingModule({
      imports: [GoogleLinkComponent],
      providers: [{ provide: GoogleIntegrationService, useValue: mockService }],
    }).compileComponents();

    fixture = TestBed.createComponent(GoogleLinkComponent);
    component = fixture.componentInstance;
    nativeElement = fixture.nativeElement;
  });

  function render(): void {
    fixture.detectChanges();
  }

  function queryByTestId(testId: string): HTMLElement | null {
    return nativeElement.querySelector(`[data-testid="${testId}"]`);
  }

  it('should create', () => {
    render();
    expect(component).toBeTruthy();
  });

  it('should show "Not connected" when not linked', () => {
    render();
    const text = nativeElement.textContent;
    expect(text).toContain('Not connected');
  });

  it('should show "Connected" badge when linked', () => {
    (mockService.linkedAccounts as ReturnType<typeof signal<LinkedAccount[]>>).set([mockAccount]);
    render();
    expect(queryByTestId('google-link-status-connected')).toBeTruthy();
    expect(queryByTestId('google-link-status-connected')?.textContent?.trim()).toBe('Connected');
  });

  it('should show email when linked', () => {
    (mockService.linkedAccounts as ReturnType<typeof signal<LinkedAccount[]>>).set([mockAccount]);
    render();
    expect(queryByTestId('google-link-email')?.textContent?.trim()).toBe('test@gmail.com');
  });

  it('should show "Link Google Account" button when not linked', () => {
    render();
    const linkButton = queryByTestId('link-google');
    expect(linkButton).toBeTruthy();
    expect(linkButton?.textContent?.trim()).toContain('Link Google Account');
  });

  it('should show "Unlink Google Account" button when linked', () => {
    (mockService.linkedAccounts as ReturnType<typeof signal<LinkedAccount[]>>).set([mockAccount]);
    render();
    const unlinkButton = queryByTestId('unlink-google');
    expect(unlinkButton).toBeTruthy();
    expect(unlinkButton?.textContent?.trim()).toContain('Unlink Google Account');
  });

  it('should call linkGoogle() on link button click', () => {
    render();
    queryByTestId('link-google')?.click();
    expect(mockService.linkGoogle).toHaveBeenCalledTimes(1);
  });

  it('should call unlinkGoogle() on unlink button click', () => {
    (mockService.linkedAccounts as ReturnType<typeof signal<LinkedAccount[]>>).set([mockAccount]);
    render();
    queryByTestId('unlink-google')?.click();
    expect(mockService.unlinkGoogle).toHaveBeenCalledTimes(1);
  });

  it('should show error message when error signal is set', () => {
    (mockService.error as ReturnType<typeof signal<string | null>>).set('Something went wrong');
    render();
    const errorEl = queryByTestId('google-link-error');
    expect(errorEl).toBeTruthy();
    expect(errorEl?.textContent?.trim()).toBe('Something went wrong');
  });

  it('should call loadLinkedAccounts on init', () => {
    render();
    expect(mockService.loadLinkedAccounts).toHaveBeenCalledTimes(1);
  });
});
