import { TestBed } from '@angular/core/testing';
import { ToastService } from './toast.service';

describe('ToastService', () => {
  let service: ToastService;

  beforeEach(() => {
    vi.useFakeTimers();

    TestBed.configureTestingModule({
      providers: [ToastService],
    });

    service = TestBed.inject(ToastService);
  });

  afterEach(() => {
    vi.useRealTimers();
  });

  it('should start with empty toasts', () => {
    expect(service.toasts()).toEqual([]);
  });

  it('should add a success toast', () => {
    service.success('Saved!');

    expect(service.toasts()).toHaveLength(1);
    expect(service.toasts()[0]).toEqual(
      expect.objectContaining({ message: 'Saved!', type: 'success' }),
    );
  });

  it('should add an error toast', () => {
    service.error('Something went wrong');

    expect(service.toasts()).toHaveLength(1);
    expect(service.toasts()[0]).toEqual(
      expect.objectContaining({ message: 'Something went wrong', type: 'error' }),
    );
  });

  it('should auto-dismiss after timeout', () => {
    service.success('Temporary');

    expect(service.toasts()).toHaveLength(1);

    vi.advanceTimersByTime(4000);

    expect(service.toasts()).toHaveLength(0);
  });

  it('should dismiss a specific toast', () => {
    service.success('First');
    service.error('Second');

    const firstId = service.toasts()[0].id;
    service.dismiss(firstId);

    expect(service.toasts()).toHaveLength(1);
    expect(service.toasts()[0].message).toBe('Second');
  });

  it('should handle multiple toasts', () => {
    service.success('One');
    service.success('Two');
    service.error('Three');

    expect(service.toasts()).toHaveLength(3);
    expect(service.toasts().map((t) => t.message)).toEqual(['One', 'Two', 'Three']);
  });

  it('should assign unique ids', () => {
    service.success('A');
    service.success('B');

    const ids = service.toasts().map((t) => t.id);
    expect(ids[0]).not.toBe(ids[1]);
  });
});
