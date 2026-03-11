import { ComponentFixture, TestBed } from '@angular/core/testing';
import { of, throwError } from 'rxjs';
import { FederalStatesPageComponent } from './federal-states-page.component';
import { BaseDataService, FederalStateDto } from '../../services/base-data.service';

const MOCK_STATES: FederalStateDto[] = [
  { id: '1', name: 'Bayern', iso3166Code: 'DE-BY' },
  { id: '2', name: 'Berlin', iso3166Code: 'DE-BE' },
  { id: '3', name: 'Hamburg', iso3166Code: 'DE-HH' },
];

describe('FederalStatesPageComponent', () => {
  let fixture: ComponentFixture<FederalStatesPageComponent>;
  let component: FederalStatesPageComponent;
  let baseDataSpy: { getFederalStates: ReturnType<typeof vi.fn> };

  beforeEach(() => {
    baseDataSpy = { getFederalStates: vi.fn().mockReturnValue(of(MOCK_STATES)) };

    TestBed.configureTestingModule({
      imports: [FederalStatesPageComponent],
      providers: [{ provide: BaseDataService, useValue: baseDataSpy }],
    });

    fixture = TestBed.createComponent(FederalStatesPageComponent);
    component = fixture.componentInstance;
  });

  it('should create', () => {
    fixture.detectChanges();
    expect(component).toBeTruthy();
  });

  it('loads federal states on init', () => {
    fixture.detectChanges();
    expect(baseDataSpy.getFederalStates).toHaveBeenCalled();
    expect(component.federalStates()).toEqual(MOCK_STATES);
    expect(component.isLoading()).toBe(false);
  });

  it('starts with loading state true', () => {
    expect(component.isLoading()).toBe(true);
  });

  it('sets loading to false after successful load', () => {
    fixture.detectChanges();
    expect(component.isLoading()).toBe(false);
  });

  it('sets loading to false on error', () => {
    baseDataSpy.getFederalStates.mockReturnValue(throwError(() => new Error('Network error')));
    fixture.detectChanges();
    expect(component.isLoading()).toBe(false);
    expect(component.federalStates()).toEqual([]);
  });

  it('renders federal state list component', () => {
    fixture.detectChanges();
    const el: HTMLElement = fixture.nativeElement;
    const list = el.querySelector('app-federal-state-list');
    expect(list).toBeTruthy();
  });
});
