import { ComponentFixture, TestBed } from '@angular/core/testing';
import { AvatarUploadComponent } from './avatar-upload.component';
import { AvatarService } from './avatar.service';
import { of, throwError } from 'rxjs';

describe('AvatarUploadComponent', () => {
  let component: AvatarUploadComponent;
  let fixture: ComponentFixture<AvatarUploadComponent>;
  let nativeElement: HTMLElement;
  let mockAvatarService: { uploadAvatar: ReturnType<typeof vi.fn> };

  beforeEach(async () => {
    mockAvatarService = {
      uploadAvatar: vi.fn().mockReturnValue(of('avatar-id-123')),
    };

    await TestBed.configureTestingModule({
      imports: [AvatarUploadComponent],
      providers: [{ provide: AvatarService, useValue: mockAvatarService }],
    }).compileComponents();

    fixture = TestBed.createComponent(AvatarUploadComponent);
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

  it('should show drop zone initially', () => {
    render();
    const dropZone = nativeElement.querySelector('.border-dashed');
    expect(dropZone).toBeTruthy();
    expect(nativeElement.textContent).toContain('Click or drag image here');
  });

  it('should not show preview initially', () => {
    render();
    const preview = nativeElement.querySelector('img[alt="Avatar preview"]');
    expect(preview).toBeNull();
  });

  it('should reject files with invalid MIME type', () => {
    render();
    const file = new File(['content'], 'test.gif', { type: 'image/gif' });
    component.onFileSelected(createFileEvent(file));
    fixture.detectChanges();

    expect(nativeElement.textContent).toContain('Only JPEG, PNG, and WebP images are supported.');
  });

  it('should reject files exceeding 5 MB', () => {
    render();
    const largeContent = new Uint8Array(6 * 1024 * 1024); // 6 MB
    const file = new File([largeContent], 'large.jpg', { type: 'image/jpeg' });
    component.onFileSelected(createFileEvent(file));
    fixture.detectChanges();

    expect(nativeElement.textContent).toContain('Image must not exceed 5 MB.');
  });

  it('should accept valid JPEG file', async () => {
    render();
    const file = new File(['jpeg-content'], 'test.jpg', { type: 'image/jpeg' });
    component.onFileSelected(createFileEvent(file));

    // Wait for FileReader async operation
    await new Promise((resolve) => setTimeout(resolve, 50));
    fixture.detectChanges();

    expect(component.previewUrl()).toBeTruthy();
  });

  it('should accept valid PNG file', async () => {
    render();
    const file = new File(['png-content'], 'test.png', { type: 'image/png' });
    component.onFileSelected(createFileEvent(file));

    await new Promise((resolve) => setTimeout(resolve, 50));
    fixture.detectChanges();

    expect(component.previewUrl()).toBeTruthy();
  });

  it('should accept valid WebP file', async () => {
    render();
    const file = new File(['webp-content'], 'test.webp', { type: 'image/webp' });
    component.onFileSelected(createFileEvent(file));

    await new Promise((resolve) => setTimeout(resolve, 50));
    fixture.detectChanges();

    expect(component.previewUrl()).toBeTruthy();
  });

  it('should clear preview on cancel', async () => {
    render();
    const file = new File(['content'], 'test.jpg', { type: 'image/jpeg' });
    component.onFileSelected(createFileEvent(file));

    await new Promise((resolve) => setTimeout(resolve, 50));
    fixture.detectChanges();

    component.cancelPreview();
    fixture.detectChanges();

    expect(component.previewUrl()).toBeNull();
    expect(nativeElement.querySelector('.border-dashed')).toBeTruthy();
  });

  it('should emit avatarUploaded on successful upload', async () => {
    render();
    const spy = vi.fn();
    component.avatarUploaded.subscribe(spy);

    const file = new File(['content'], 'test.jpg', { type: 'image/jpeg' });
    component.onFileSelected(createFileEvent(file));

    await new Promise((resolve) => setTimeout(resolve, 50));
    fixture.detectChanges();

    component.uploadAvatar();

    // Wait for the second FileReader read + service call
    await new Promise((resolve) => setTimeout(resolve, 100));
    fixture.detectChanges();

    expect(mockAvatarService.uploadAvatar).toHaveBeenCalled();
    expect(spy).toHaveBeenCalledWith('avatar-id-123');
  });

  it('should show error message on upload failure', async () => {
    mockAvatarService.uploadAvatar.mockReturnValue(of(null));
    render();

    const file = new File(['content'], 'test.jpg', { type: 'image/jpeg' });
    component.onFileSelected(createFileEvent(file));

    await new Promise((resolve) => setTimeout(resolve, 50));
    fixture.detectChanges();

    component.uploadAvatar();

    await new Promise((resolve) => setTimeout(resolve, 100));
    fixture.detectChanges();

    expect(nativeElement.textContent).toContain('Failed to upload avatar');
  });

  it('should show error message on network error', async () => {
    mockAvatarService.uploadAvatar.mockReturnValue(throwError(() => new Error('Network error')));
    render();

    const file = new File(['content'], 'test.jpg', { type: 'image/jpeg' });
    component.onFileSelected(createFileEvent(file));

    await new Promise((resolve) => setTimeout(resolve, 50));
    fixture.detectChanges();

    component.uploadAvatar();

    await new Promise((resolve) => setTimeout(resolve, 100));
    fixture.detectChanges();

    expect(nativeElement.textContent).toContain('An error occurred');
  });

  function createFileEvent(file: File): Event {
    const input = document.createElement('input');
    input.type = 'file';
    Object.defineProperty(input, 'files', { value: [file] });
    return { target: input } as unknown as Event;
  }
});
