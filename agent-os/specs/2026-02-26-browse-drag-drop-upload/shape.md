# Browse Drag-and-Drop File Upload — Shaping Notes

**Feature**: Drag-and-drop file upload directly on the browse page content area
**Created**: 2026-02-26

---

## Scope

Enhance the File Management browse page so the entire content area (below the toolbar) acts as a drag-and-drop zone for file uploads. Users can drag files from their desktop and drop them directly onto the browse page without clicking the Upload button first.

### What's In Scope

1. **Full-area drop zone**: Content area below toolbar accepts file drops
2. **Drag overlay**: Semi-transparent blue overlay with upload icon and text during drag
3. **Auto-open upload dialog**: Dropped files automatically open the existing upload dialog with files pre-loaded
4. **Drag counter pattern**: Prevent overlay flickering from nested element drag events
5. **i18n support**: Localized "Drop files to upload" text

### What's Out of Scope

1. Drag-and-drop between folders (file reordering) — separate feature
2. Folder upload via drag (directory dropping) — future enhancement
3. Upload progress bar outside the dialog (inline toast) — not selected
4. Drop zone on other pages (albums, search, inbox) — future enhancement

---

## Decisions

- **UX Pattern**: Full-page overlay with icon (Google Drive / Dropbox style)
- **Drop zone scope**: Content area only (below toolbar), not the entire page
- **On drop**: Auto-open existing upload dialog with files already uploading
- **Drag handling**: Drag counter pattern to prevent flickering from nested elements
- **Upload button**: Kept as fallback — this is an additive UX improvement
- **No new components**: Overlay is inline template in BrowsePageComponent
- **No new libraries**: Uses native HTML5 Drag and Drop API (already used in upload dialog)

---

## Context

- **Visuals**: None provided; ASCII mockups from shaping session are sufficient
- **References**: Existing `UploadDialogComponent` at `src/frontend/family-hub-web/src/app/features/file-management/components/browse/upload-dialog/upload-dialog.component.ts`
- **Product alignment**: N/A (no product folder exists)

## Standards Applied

- **frontend/angular-components** — Standalone components with signals-based state
