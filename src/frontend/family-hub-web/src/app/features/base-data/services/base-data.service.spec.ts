import { TestBed } from '@angular/core/testing';
import { Apollo } from 'apollo-angular';
import { of, throwError } from 'rxjs';
import { BaseDataService, FederalStateDto } from './base-data.service';

const MOCK_STATES: FederalStateDto[] = [
  { id: '1', name: 'Bayern', iso3166Code: 'DE-BY' },
  { id: '2', name: 'Berlin', iso3166Code: 'DE-BE' },
];

describe('BaseDataService', () => {
  let service: BaseDataService;
  let apolloSpy: { query: ReturnType<typeof vi.fn> };

  beforeEach(() => {
    apolloSpy = { query: vi.fn() };

    TestBed.configureTestingModule({
      providers: [BaseDataService, { provide: Apollo, useValue: apolloSpy }],
    });

    service = TestBed.inject(BaseDataService);
  });

  describe('getFederalStates', () => {
    it('returns federal states from GraphQL query', () => {
      apolloSpy.query.mockReturnValue(of({ data: { baseData: { federalStates: MOCK_STATES } } }));

      let result: FederalStateDto[] = [];
      service.getFederalStates().subscribe((states) => (result = states));

      expect(apolloSpy.query).toHaveBeenCalledWith(
        expect.objectContaining({ fetchPolicy: 'cache-first' }),
      );
      expect(result).toEqual(MOCK_STATES);
    });

    it('returns empty array when federalStates is null', () => {
      apolloSpy.query.mockReturnValue(of({ data: { baseData: { federalStates: null } } }));

      let result: FederalStateDto[] | undefined;
      service.getFederalStates().subscribe((states) => (result = states));
      expect(result).toEqual([]);
    });

    it('returns empty array when baseData is null', () => {
      apolloSpy.query.mockReturnValue(of({ data: { baseData: null } }));

      let result: FederalStateDto[] | undefined;
      service.getFederalStates().subscribe((states) => (result = states));
      expect(result).toEqual([]);
    });

    it('returns empty array on error', () => {
      apolloSpy.query.mockReturnValue(throwError(() => new Error('Network error')));

      let result: FederalStateDto[] | undefined;
      service.getFederalStates().subscribe((states) => (result = states));
      expect(result).toEqual([]);
    });
  });

  describe('getFederalStateByIso3166', () => {
    it('returns a single federal state by ISO code', () => {
      apolloSpy.query.mockReturnValue(
        of({ data: { baseData: { federalStateByIso3166: MOCK_STATES[0] } } }),
      );

      let result: FederalStateDto | null = null;
      service.getFederalStateByIso3166('DE-BY').subscribe((state) => (result = state));

      expect(apolloSpy.query).toHaveBeenCalledWith(
        expect.objectContaining({ variables: { code: 'DE-BY' } }),
      );
      expect(result).toEqual(MOCK_STATES[0]);
    });

    it('returns null when state not found', () => {
      apolloSpy.query.mockReturnValue(of({ data: { baseData: { federalStateByIso3166: null } } }));

      let result: FederalStateDto | null = MOCK_STATES[0];
      service.getFederalStateByIso3166('DE-XX').subscribe((state) => (result = state));
      expect(result).toBeNull();
    });

    it('returns null on error', () => {
      apolloSpy.query.mockReturnValue(throwError(() => new Error('Network error')));

      let result: FederalStateDto | null = MOCK_STATES[0];
      service.getFederalStateByIso3166('DE-BY').subscribe((state) => (result = state));
      expect(result).toBeNull();
    });
  });
});
