# Drag-and-Drop File Upload on Browse Page

## Context

Currently, the File Management browse page requires users to click the "Upload" button to open the upload dialog, then drag files into it. The user wants to drag and drop files **directly onto the browse page content area** — the whole area should be a drop zone, eliminating the need for the upload button click. This is the standard UX pattern used by Google Drive, Dropbox, and OneDrive.

**This is a frontend-only change.** No backend modifications needed.

## Decisions

- **Drop zone scope:** Content area only (below toolbar), not the full page
- **Drag visual feedback:** Full-page overlay with upload icon and "Drop files to upload" text (semi-transparent blue)
- **On drop behavior:** Auto-open the existing upload dialog with dropped files already uploading
- **Drag event handling:** Use drag counter pattern to avoid flickering from nested element events
- **Upload button:** Keep the Upload button in the toolbar as a fallback (some users prefer click-to-browse)

## Files to Modify

### 1. `src/frontend/family-hub-web/src/app/features/file-management/components/browse/browse-page.component.ts`

**Changes:**

- Add `isDragging = signal(false)` and `private dragCounter = 0` for drag state
- Add `onContentDragEnter()`, `onContentDragOver()`, `onContentDragLeave()`, `onContentDrop()` methods
- Bind drag event handlers to the content area `<div class="flex-1 overflow-auto">`
- Add a drag overlay template inside the content area (shown conditionally via `@if (isDragging())`)
- Add `droppedFiles = signal<File[]>([])` to pass files to upload dialog
- Modify the upload dialog opening logic: when files are dropped, set `droppedFiles`, then set `showUploadDialog(true)`
- Pass `[initialFiles]="droppedFiles()"` to `<app-upload-dialog>`
- Clear `droppedFiles` when upload dialog closes

**Drag event handler logic:**

```
onContentDragEnter(event):
  event.preventDefault()
  dragCounter++
  isDragging.set(true)

onContentDragOver(event):
  event.preventDefault()  // Required to allow drop

onContentDragLeave(event):
  event.preventDefault()
  dragCounter--
  if (dragCounter === 0) isDragging.set(false)

onContentDrop(event):
  event.preventDefault()
  dragCounter = 0
  isDragging.set(false)
  files = Array.from(event.dataTransfer.files)
  if (files.length > 0):
    droppedFiles.set(files)
    showUploadDialog.set(true)
```

**Overlay template (inside the content area div):**

```html
@if (isDragging()) {
  <div class="absolute inset-0 bg-blue-500/20 border-2 border-dashed border-blue-400 rounded-lg flex items-center justify-center z-10 pointer-events-none">
    <div class="text-center">
      <div class="upload-icon mb-2"><!-- ICONS.UPLOAD scaled large --></div>
      <p class="text-blue-700 font-semibold text-lg">Drop files to upload</p>
    </div>
  </div>
}
```

Note: The content area div needs `relative` positioning for the absolute overlay to work.

### 2. `src/frontend/family-hub-web/src/app/features/file-management/components/browse/upload-dialog/upload-dialog.component.ts`

**Changes:**

- Add `initialFiles = input<File[]>([])` — accepts files passed from the browse page
- Add an `effect()` or `ngOnInit` check: if `initialFiles()` has files, call `addFiles()` immediately on initialization
- This way, when the dialog opens with pre-dropped files, upload starts automatically

**Implementation detail:**

```typescript
readonly initialFiles = input<File[]>([]);

constructor(sanitizer: DomSanitizer) {
  // existing icon setup...

  effect(() => {
    const files = this.initialFiles();
    if (files.length > 0) {
      this.addFiles(files);
    }
  });
}
```

## Implementation Tasks

### Task 1: Modify UploadDialogComponent to Accept Initial Files

- Add `initialFiles` input signal
- Add `effect()` to auto-process initial files on open
- Ensure `addFiles()` is not called for empty arrays

### Task 2: Add Drag-and-Drop Zone to BrowsePageComponent

- Add drag state signals and counter
- Add drag event handlers on content area
- Add overlay template
- Wire up file passing to upload dialog via `droppedFiles` signal and `[initialFiles]` binding
- Clear `droppedFiles` on dialog close
- Add `position: relative` to the content area for overlay positioning

### Task 3: Add i18n Labels

- Add `@@files.browse.dropToUpload` for "Drop files to upload" text

## Verification

1. **Manual test:** Drag a file from the desktop over the browse page content area
   - Overlay should appear with blue tint and "Drop files to upload" message
   - Overlay should NOT flicker when moving over child elements (folders, files)
   - Overlay should disappear when dragging out of the content area
2. **Drop test:** Drop files on the content area
   - Upload dialog should open automatically
   - Dropped files should appear in the dialog and start uploading immediately
3. **Upload button still works:** Click the Upload button in the toolbar
   - Dialog should open normally with empty state (drag-and-drop zone + browse button)
4. **Edge cases:**
   - Dragging non-file items (text, links) should not trigger the overlay or should handle gracefully
   - Multiple files dropped at once should all be added to the upload queue
   - Drag over toolbar area should NOT trigger overlay
