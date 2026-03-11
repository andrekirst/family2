import { ComponentFixture, TestBed } from '@angular/core/testing';
import { FederalStateListComponent } from './federal-state-list.component';
import { FederalStateDto } from '../../services/base-data.service';

const MOCK_STATES: FederalStateDto[] = [
  { id: '1', name: 'Bayern', iso3166Code: 'DE-BY' },
  { id: '2', name: 'Berlin', iso3166Code: 'DE-BE' },
  { id: '3', name: 'Hamburg', iso3166Code: 'DE-HH' },
];

describe('FederalStateListComponent', () => {
  let fixture: ComponentFixture<FederalStateListComponent>;
  let component: FederalStateListComponent;

  beforeEach(() => {
    TestBed.configureTestingModule({
      imports: [FederalStateListComponent],
    });

    fixture = TestBed.createComponent(FederalStateListComponent);
    component = fixture.componentInstance;
  });

  it('should create', () => {
    fixture.detectChanges();
    expect(component).toBeTruthy();
  });

  it('shows loading skeleton when isLoading is true', () => {
    fixture.componentRef.setInput('isLoading', true);
    fixture.componentRef.setInput('federalStates', []);
    fixture.detectChanges();

    const el: HTMLElement = fixture.nativeElement;
    expect(el.querySelector('.animate-pulse')).toBeTruthy();
    expect(el.querySelector('table')).toBeFalsy();
  });

  it('shows empty state when no federal states and not loading', () => {
    fixture.componentRef.setInput('isLoading', false);
    fixture.componentRef.setInput('federalStates', []);
    fixture.detectChanges();

    const el: HTMLElement = fixture.nativeElement;
    expect(el.querySelector('.animate-pulse')).toBeFalsy();
    expect(el.querySelector('table')).toBeFalsy();
    expect(el.textContent).toContain('No federal states found');
  });

  it('renders table with federal states data', () => {
    fixture.componentRef.setInput('isLoading', false);
    fixture.componentRef.setInput('federalStates', MOCK_STATES);
    fixture.detectChanges();

    const el: HTMLElement = fixture.nativeElement;
    const table = el.querySelector('table');
    expect(table).toBeTruthy();

    const rows = el.querySelectorAll('tbody tr');
    expect(rows.length).toBe(3);
  });

  it('displays state names in table rows', () => {
    fixture.componentRef.setInput('isLoading', false);
    fixture.componentRef.setInput('federalStates', MOCK_STATES);
    fixture.detectChanges();

    const el: HTMLElement = fixture.nativeElement;
    expect(el.textContent).toContain('Bayern');
    expect(el.textContent).toContain('Berlin');
    expect(el.textContent).toContain('Hamburg');
  });

  it('displays ISO codes in table rows', () => {
    fixture.componentRef.setInput('isLoading', false);
    fixture.componentRef.setInput('federalStates', MOCK_STATES);
    fixture.detectChanges();

    const el: HTMLElement = fixture.nativeElement;
    expect(el.textContent).toContain('DE-BY');
    expect(el.textContent).toContain('DE-BE');
    expect(el.textContent).toContain('DE-HH');
  });

  it('renders table headers for Name and ISO Code', () => {
    fixture.componentRef.setInput('isLoading', false);
    fixture.componentRef.setInput('federalStates', MOCK_STATES);
    fixture.detectChanges();

    const el: HTMLElement = fixture.nativeElement;
    const headers = el.querySelectorAll('th');
    expect(headers.length).toBe(2);
  });
});
