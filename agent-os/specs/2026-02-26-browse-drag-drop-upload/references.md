# References for Browse Drag-and-Drop File Upload

## Existing Implementations

### Upload Dialog Component

- **Location:** `src/frontend/family-hub-web/src/app/features/file-management/components/browse/upload-dialog/upload-dialog.component.ts`
- **Relevance:** Contains the existing drag-and-drop upload zone (inside the dialog modal). This component will be extended to accept initial files via input signal.
- **Key patterns:**
  - HTML5 native drag-and-drop (`dragover`, `dragleave`, `drop` events)
  - `isDragging` signal for visual feedback
  - `addFiles()` method for processing dropped files
  - `processUpload()` for two-stage upload (HTTP upload → GraphQL registration)

### Browse Page Component

- **Location:** `src/frontend/family-hub-web/src/app/features/file-management/components/browse/browse-page.component.ts`
- **Relevance:** The main page that will receive the full-area drop zone. Currently hosts the upload dialog via conditional rendering.
- **Key patterns:**
  - `showUploadDialog` signal controls dialog visibility
  - `refreshContent()` called after successful uploads
  - Content area is `<div class="flex-1 overflow-auto">` — this becomes the drop zone

### File Upload Service

- **Location:** `src/frontend/family-hub-web/src/app/features/file-management/services/file-upload.service.ts`
- **Relevance:** Handles chunked and simple uploads. No changes needed — reused as-is.
- **Key patterns:**
  - Simple upload for files < 5MB
  - Chunked upload for files >= 5MB (5MB chunks)
  - Progress tracking via `Observable<UploadProgress>`

## External References

- **Google Drive** — Full-page drop zone with blue overlay and "Drop files here to upload" text
- **Dropbox** — Similar pattern with full-area drop zone
- **HTML5 Drag Counter Pattern** — Standard technique to prevent flickering from nested element `dragenter`/`dragleave` events
