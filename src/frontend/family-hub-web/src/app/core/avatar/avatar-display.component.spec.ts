import { ComponentFixture, TestBed } from '@angular/core/testing';
import { AvatarDisplayComponent } from './avatar-display.component';
import { EnvironmentConfigService } from '../config/environment-config.service';

describe('AvatarDisplayComponent', () => {
  let component: AvatarDisplayComponent;
  let fixture: ComponentFixture<AvatarDisplayComponent>;
  let nativeElement: HTMLElement;

  const mockEnvConfig = {
    apiBaseUrl: 'https://api.example.com',
    apiUrl: 'https://api.example.com/graphql',
    keycloak: {
      issuer: '',
      clientId: '',
      redirectUri: '',
      postLogoutRedirectUri: '',
      scope: '',
    },
  };

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [AvatarDisplayComponent],
      providers: [{ provide: EnvironmentConfigService, useValue: mockEnvConfig }],
    }).compileComponents();

    fixture = TestBed.createComponent(AvatarDisplayComponent);
    component = fixture.componentInstance;
    nativeElement = fixture.nativeElement;
  });

  function render(): void {
    fixture.detectChanges();
  }

  it('should create', () => {
    render();
    expect(component).toBeTruthy();
  });

  it('should show initials fallback when no avatarId', () => {
    fixture.componentRef.setInput('name', 'John Doe');
    render();

    const initialsEl = nativeElement.querySelector('div.rounded-full');
    expect(initialsEl).toBeTruthy();
    expect(initialsEl?.textContent?.trim()).toBe('JD');
  });

  it('should show single-name initials correctly', () => {
    fixture.componentRef.setInput('name', 'Alice');
    render();

    const initialsEl = nativeElement.querySelector('div.rounded-full');
    expect(initialsEl?.textContent?.trim()).toBe('AL');
  });

  it('should show ? for empty name', () => {
    fixture.componentRef.setInput('name', '');
    render();

    const initialsEl = nativeElement.querySelector('div.rounded-full');
    expect(initialsEl?.textContent?.trim()).toBe('?');
  });

  it('should render img when avatarId is provided', () => {
    fixture.componentRef.setInput('avatarId', 'abc-123');
    fixture.componentRef.setInput('name', 'John');
    fixture.componentRef.setInput('size', 'small');
    render();

    const img = nativeElement.querySelector('img');
    expect(img).toBeTruthy();
    expect(img?.getAttribute('src')).toBe('https://api.example.com/api/avatars/abc-123/small');
  });

  it('should set correct dimensions for small size', () => {
    fixture.componentRef.setInput('name', 'Jane');
    fixture.componentRef.setInput('size', 'small');
    render();

    const el = nativeElement.querySelector('div.rounded-full') as HTMLElement;
    expect(el?.style.width).toBe('48px');
    expect(el?.style.height).toBe('48px');
  });

  it('should set correct dimensions for tiny size', () => {
    fixture.componentRef.setInput('name', 'Jane');
    fixture.componentRef.setInput('size', 'tiny');
    render();

    const el = nativeElement.querySelector('div.rounded-full') as HTMLElement;
    expect(el?.style.width).toBe('24px');
    expect(el?.style.height).toBe('24px');
  });

  it('should generate deterministic background color', () => {
    fixture.componentRef.setInput('name', 'John Doe');
    render();

    const el = nativeElement.querySelector('div.rounded-full') as HTMLElement;
    expect(el?.style.backgroundColor).toBeTruthy();

    // Same name should always produce the same color
    const color1 = el?.style.backgroundColor;

    // Create another component with same name
    const fixture2 = TestBed.createComponent(AvatarDisplayComponent);
    fixture2.componentRef.setInput('name', 'John Doe');
    fixture2.detectChanges();
    const el2 = fixture2.nativeElement.querySelector('div.rounded-full') as HTMLElement;
    expect(el2?.style.backgroundColor).toBe(color1);
  });

  it('should use last name initial for multi-word names', () => {
    fixture.componentRef.setInput('name', 'John Michael Doe');
    render();

    const initialsEl = nativeElement.querySelector('div.rounded-full');
    expect(initialsEl?.textContent?.trim()).toBe('JD');
  });
});
