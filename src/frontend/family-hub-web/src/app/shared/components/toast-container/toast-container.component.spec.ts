import { ComponentFixture, TestBed } from '@angular/core/testing';
import { ToastContainerComponent } from './toast-container.component';
import { ToastService } from '../../services/toast.service';

describe('ToastContainerComponent', () => {
  let fixture: ComponentFixture<ToastContainerComponent>;
  let nativeElement: HTMLElement;
  let toastService: ToastService;

  beforeEach(async () => {
    vi.useFakeTimers();

    await TestBed.configureTestingModule({
      imports: [ToastContainerComponent],
    }).compileComponents();

    toastService = TestBed.inject(ToastService);
    fixture = TestBed.createComponent(ToastContainerComponent);
    nativeElement = fixture.nativeElement;
    fixture.detectChanges();
  });

  afterEach(() => {
    vi.useRealTimers();
  });

  function queryAllToasts(): HTMLElement[] {
    return Array.from(nativeElement.querySelectorAll('[data-testid="toast"]'));
  }

  it('should render nothing when no toasts', () => {
    expect(queryAllToasts()).toHaveLength(0);
  });

  it('should render a success toast', () => {
    toastService.success('Changes saved');
    fixture.detectChanges();

    const toasts = queryAllToasts();
    expect(toasts).toHaveLength(1);
    expect(toasts[0].textContent).toContain('Changes saved');
    expect(toasts[0].classList).toContain('bg-gray-900');
  });

  it('should render an error toast', () => {
    toastService.error('Something failed');
    fixture.detectChanges();

    const toasts = queryAllToasts();
    expect(toasts).toHaveLength(1);
    expect(toasts[0].textContent).toContain('Something failed');
    expect(toasts[0].classList).toContain('bg-red-600');
  });

  it('should render multiple toasts', () => {
    toastService.success('First');
    toastService.error('Second');
    toastService.success('Third');
    fixture.detectChanges();

    expect(queryAllToasts()).toHaveLength(3);
  });
});
