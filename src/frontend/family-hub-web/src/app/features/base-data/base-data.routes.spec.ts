import { BASE_DATA_ROUTES } from './base-data.routes';
import { BaseDataLayoutComponent } from './components/base-data-layout/base-data-layout.component';

describe('BASE_DATA_ROUTES', () => {
  const rootRoute = BASE_DATA_ROUTES[0];

  it('has a root route with BaseDataLayoutComponent', () => {
    expect(rootRoute.path).toBe('');
    expect(rootRoute.component).toBe(BaseDataLayoutComponent);
  });

  it('defines child routes', () => {
    expect(rootRoute.children).toBeDefined();
    expect(rootRoute.children!.length).toBe(2);
  });

  it('redirects empty path to federal-states', () => {
    const redirectRoute = rootRoute.children![0];
    expect(redirectRoute.path).toBe('');
    expect(redirectRoute.redirectTo).toBe('federal-states');
    expect(redirectRoute.pathMatch).toBe('full');
  });

  it('has a federal-states child route with lazy-loaded component', () => {
    const federalStatesRoute = rootRoute.children![1];
    expect(federalStatesRoute.path).toBe('federal-states');
    expect(federalStatesRoute.loadComponent).toBeDefined();
    expect(typeof federalStatesRoute.loadComponent).toBe('function');
  });

  it('lazy-loads FederalStatesPageComponent', async () => {
    const federalStatesRoute = rootRoute.children![1];
    const component = await federalStatesRoute.loadComponent!();
    expect(component).toBeDefined();
  });
});
