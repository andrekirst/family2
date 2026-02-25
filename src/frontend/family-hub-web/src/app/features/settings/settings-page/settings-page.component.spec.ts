import { ComponentFixture, TestBed } from '@angular/core/testing';
import { ActivatedRoute } from '@angular/router';
import { of } from 'rxjs';
import { SettingsPageComponent } from './settings-page.component';
import { I18nService } from '../../../core/i18n/i18n.service';
import { FormatPreferencesService } from '../../../core/i18n/format-preferences.service';
import { UserService } from '../../../core/user/user.service';
import { TopBarService } from '../../../shared/services/top-bar.service';
import { GoogleIntegrationService } from '../services/google-integration.service';
import { signal } from '@angular/core';

describe('SettingsPageComponent', () => {
  let fixture: ComponentFixture<SettingsPageComponent>;
  let component: SettingsPageComponent;
  let i18nSpy: {
    currentLocale: ReturnType<typeof signal>;
    switchLocale: ReturnType<typeof vi.fn>;
    supportedLocales: string[];
  };
  let formatPrefsSpy: {
    dateFormat: ReturnType<typeof signal>;
    timeFormat: ReturnType<typeof signal>;
    setDateFormat: ReturnType<typeof vi.fn>;
    setTimeFormat: ReturnType<typeof vi.fn>;
  };
  let userServiceSpy: { currentUser: ReturnType<typeof signal> };
  let topBarSpy: { setConfig: ReturnType<typeof vi.fn> };
  let googleServiceSpy: Record<string, unknown>;

  beforeEach(() => {
    i18nSpy = {
      currentLocale: signal('en'),
      switchLocale: vi.fn(),
      supportedLocales: ['en', 'de'],
    };
    formatPrefsSpy = {
      dateFormat: signal('MM/DD/YYYY' as const),
      timeFormat: signal('12h' as const),
      setDateFormat: vi.fn(),
      setTimeFormat: vi.fn(),
    };
    userServiceSpy = {
      currentUser: signal({ id: '1', email: 'test@example.com', name: 'Test User' }),
    };
    topBarSpy = { setConfig: vi.fn() };
    googleServiceSpy = {
      isLinked: signal(false),
      primaryAccount: signal(null),
      linkedAccounts: signal([]),
      syncStatus: signal(null),
      loading: signal(false),
      error: signal(null),
      loadLinkedAccounts: vi.fn(),
      loadSyncStatus: vi.fn(),
      linkGoogle: vi.fn(),
      unlinkGoogle: vi.fn(),
      refreshToken: vi.fn(),
    };

    TestBed.configureTestingModule({
      imports: [SettingsPageComponent],
      providers: [
        { provide: I18nService, useValue: i18nSpy },
        { provide: FormatPreferencesService, useValue: formatPrefsSpy },
        { provide: UserService, useValue: userServiceSpy },
        { provide: TopBarService, useValue: topBarSpy },
        { provide: ActivatedRoute, useValue: { queryParams: of({}) } },
        { provide: GoogleIntegrationService, useValue: googleServiceSpy },
      ],
    });

    fixture = TestBed.createComponent(SettingsPageComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });

  it('sets top bar title on init', () => {
    expect(topBarSpy.setConfig).toHaveBeenCalledWith(
      expect.objectContaining({ title: expect.any(String) }),
    );
  });

  it('renders profile information', () => {
    const el: HTMLElement = fixture.nativeElement;
    expect(el.textContent).toContain('Test User');
    expect(el.textContent).toContain('test@example.com');
  });

  it('renders language buttons', () => {
    const el: HTMLElement = fixture.nativeElement;
    const langEn = el.querySelector('[data-testid="lang-en"]');
    const langDe = el.querySelector('[data-testid="lang-de"]');
    expect(langEn).toBeTruthy();
    expect(langDe).toBeTruthy();
  });

  it('calls switchLocale when language button is clicked', () => {
    const el: HTMLElement = fixture.nativeElement;
    const langDe = el.querySelector('[data-testid="lang-de"]') as HTMLButtonElement;
    langDe.click();
    expect(i18nSpy.switchLocale).toHaveBeenCalledWith('de');
  });

  it('does not call switchLocale for current locale', () => {
    const el: HTMLElement = fixture.nativeElement;
    const langEn = el.querySelector('[data-testid="lang-en"]') as HTMLButtonElement;
    langEn.click();
    expect(i18nSpy.switchLocale).not.toHaveBeenCalled();
  });

  it('renders date format buttons', () => {
    const el: HTMLElement = fixture.nativeElement;
    expect(el.querySelector('[data-testid="date-format-eu"]')).toBeTruthy();
    expect(el.querySelector('[data-testid="date-format-us"]')).toBeTruthy();
  });

  it('calls setDateFormat when date format button is clicked', () => {
    const el: HTMLElement = fixture.nativeElement;
    const btn = el.querySelector('[data-testid="date-format-eu"]') as HTMLButtonElement;
    btn.click();
    expect(formatPrefsSpy.setDateFormat).toHaveBeenCalledWith('DD.MM.YYYY');
  });

  it('renders time format buttons', () => {
    const el: HTMLElement = fixture.nativeElement;
    expect(el.querySelector('[data-testid="time-format-24h"]')).toBeTruthy();
    expect(el.querySelector('[data-testid="time-format-12h"]')).toBeTruthy();
  });

  it('calls setTimeFormat when time format button is clicked', () => {
    const el: HTMLElement = fixture.nativeElement;
    const btn = el.querySelector('[data-testid="time-format-24h"]') as HTMLButtonElement;
    btn.click();
    expect(formatPrefsSpy.setTimeFormat).toHaveBeenCalledWith('24h');
  });
});
