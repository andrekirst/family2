import { ComponentFixture, TestBed } from '@angular/core/testing';
import { RouterModule } from '@angular/router';
import { BaseDataLayoutComponent } from './base-data-layout.component';
import { TopBarService } from '../../../../shared/services/top-bar.service';

describe('BaseDataLayoutComponent', () => {
  let fixture: ComponentFixture<BaseDataLayoutComponent>;
  let component: BaseDataLayoutComponent;
  let topBarSpy: { setConfig: ReturnType<typeof vi.fn>; clear: ReturnType<typeof vi.fn> };

  beforeEach(() => {
    topBarSpy = { setConfig: vi.fn(), clear: vi.fn() };

    TestBed.configureTestingModule({
      imports: [BaseDataLayoutComponent, RouterModule.forRoot([])],
      providers: [{ provide: TopBarService, useValue: topBarSpy }],
    });

    fixture = TestBed.createComponent(BaseDataLayoutComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });

  it('sets top bar title to "Base Data" on creation', () => {
    expect(topBarSpy.setConfig).toHaveBeenCalledWith(
      expect.objectContaining({ title: expect.any(String), actions: [] }),
    );
  });

  it('clears top bar on destroy', () => {
    fixture.destroy();
    expect(topBarSpy.clear).toHaveBeenCalled();
  });

  it('defines tabs array with Federal States entry', () => {
    expect(component.tabs).toHaveLength(1);
    expect(component.tabs[0].path).toBe('federal-states');
    expect(component.tabs[0].label).toBeTruthy();
  });

  it('renders tab navigation with links', () => {
    const el: HTMLElement = fixture.nativeElement;
    const nav = el.querySelector('nav');
    expect(nav).toBeTruthy();

    const links = el.querySelectorAll('nav a');
    expect(links.length).toBe(1);
    expect(links[0].textContent?.trim()).toBeTruthy();
  });

  it('renders a router-outlet for child content', () => {
    const el: HTMLElement = fixture.nativeElement;
    const outlet = el.querySelector('router-outlet');
    expect(outlet).toBeTruthy();
  });

  it('tab links have href containing the tab path', () => {
    const el: HTMLElement = fixture.nativeElement;
    const link = el.querySelector('nav a') as HTMLAnchorElement;
    expect(link?.href).toContain('federal-states');
  });
});
