import {
  Component,
  inject,
  signal,
  OnInit,
  OnDestroy,
  ViewChild,
  TemplateRef,
} from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { DomSanitizer } from '@angular/platform-browser';
import { ICONS } from '../../../../shared/icons/icons';
import { ContextPanelService } from '../../../../shared/services/context-panel.service';
import { FileService } from '../../services/file.service';
import { FolderService } from '../../services/folder.service';
import { FileDownloadService } from '../../services/file-download.service';
import { FavoriteService } from '../../services/favorite.service';
import { FileStateService } from '../../services/file-state.service';
import { StoredFileDto } from '../../models/file.models';
import { FolderDto } from '../../models/folder.models';
import { BreadcrumbComponent } from './breadcrumb/breadcrumb.component';
import { FileGridComponent } from './file-grid/file-grid.component';
import { FileContextPanelComponent } from './file-context-panel/file-context-panel.component';
import { CreateFolderDialogComponent } from './create-folder-dialog/create-folder-dialog.component';
import { UploadDialogComponent } from './upload-dialog/upload-dialog.component';
import { MoveDialogComponent } from './move-dialog/move-dialog.component';
import { TagManagerDialogComponent } from '../tags/tag-manager-dialog.component';
import { FolderAction } from './folder-item/folder-item.component';
import { FileAction } from './file-grid-item/file-grid-item.component';

@Component({
  selector: 'app-browse-page',
  standalone: true,
  imports: [
    CommonModule,
    FormsModule,
    BreadcrumbComponent,
    FileGridComponent,
    FileContextPanelComponent,
    CreateFolderDialogComponent,
    UploadDialogComponent,
    MoveDialogComponent,
    TagManagerDialogComponent,
  ],
  template: `
    <div class="flex flex-col h-full">
      <!-- Toolbar -->
      <div class="flex items-center justify-between px-4 py-3 border-b border-gray-200 bg-white">
        <app-breadcrumb [breadcrumbs]="breadcrumbs()" (navigated)="navigateToFolder($event)" />

        <div class="flex items-center gap-2">
          <!-- View mode toggle -->
          <button
            (click)="fileState.toggleViewMode()"
            class="p-2 rounded-lg hover:bg-gray-100 text-gray-500 transition-colors"
            [attr.title]="fileState.viewMode() === 'grid' ? listViewLabel : gridViewLabel"
            data-testid="view-mode-toggle"
          >
            <span
              [innerHTML]="fileState.viewMode() === 'grid' ? icons.LIST_VIEW : icons.GRID_VIEW"
            ></span>
          </button>

          <!-- Sort dropdown -->
          <div class="relative">
            <button
              (click)="toggleSortMenu()"
              class="flex items-center gap-1 px-3 py-2 text-sm text-gray-600 rounded-lg hover:bg-gray-100 transition-colors"
              data-testid="sort-button"
            >
              <span i18n="@@files.toolbar.sort">Sort</span>
              <span [innerHTML]="icons.CHEVRON_DOWN"></span>
            </button>
            @if (showSortMenu()) {
              <div
                class="absolute right-0 top-full mt-1 w-40 bg-white border border-gray-200 rounded-lg shadow-lg z-10 py-1"
              >
                @for (option of sortOptions; track option.field) {
                  <button
                    (click)="sortBy(option.field)"
                    class="w-full text-left px-3 py-2 text-sm hover:bg-gray-100"
                    [class.text-blue-600]="fileState.sortBy() === option.field"
                    [class.font-medium]="fileState.sortBy() === option.field"
                    [class.text-gray-700]="fileState.sortBy() !== option.field"
                  >
                    {{ option.label }}
                  </button>
                }
              </div>
            }
          </div>

          <!-- Favorites toggle -->
          <button
            (click)="toggleShowFavorites()"
            class="p-2 rounded-lg hover:bg-gray-100 transition-colors"
            [class.text-yellow-500]="showFavoritesOnly()"
            [class.text-gray-500]="!showFavoritesOnly()"
            [attr.title]="showFavoritesOnly() ? showAllLabel : showFavoritesLabel"
            data-testid="favorites-toggle"
          >
            <span [innerHTML]="showFavoritesOnly() ? icons.STAR_FILLED : icons.STAR"></span>
          </button>

          <!-- Manage Tags -->
          <button
            (click)="showTagManager.set(true)"
            class="p-2 rounded-lg hover:bg-gray-100 text-gray-500 transition-colors"
            [attr.title]="manageTagsLabel"
            data-testid="manage-tags-button"
          >
            <span [innerHTML]="icons.TAG"></span>
          </button>

          <!-- Upload button -->
          <button
            (click)="showUploadDialog.set(true)"
            class="flex items-center gap-2 px-3 py-2 text-sm font-medium text-white bg-blue-600 rounded-lg hover:bg-blue-700 transition-colors"
            data-testid="upload-button"
          >
            <span [innerHTML]="icons.UPLOAD"></span>
            <span i18n="@@files.action.upload">Upload</span>
          </button>

          <!-- New Folder button -->
          <button
            (click)="showCreateFolderDialog.set(true)"
            class="flex items-center gap-2 px-3 py-2 text-sm font-medium text-gray-700 bg-white border border-gray-300 rounded-lg hover:bg-gray-50 transition-colors"
            data-testid="new-folder-button"
          >
            <span [innerHTML]="icons.PLUS"></span>
            <span i18n="@@files.action.newFolder">New Folder</span>
          </button>
        </div>
      </div>

      <!-- File grid -->
      <div class="flex-1 overflow-auto">
        @if (loading()) {
          <div class="flex items-center justify-center py-16">
            <div
              class="animate-spin rounded-full h-8 w-8 border-2 border-blue-600 border-t-transparent"
            ></div>
          </div>
        } @else {
          <app-file-grid
            [folders]="folders()"
            [files]="files()"
            [viewMode]="fileState.viewMode()"
            [selectedFileIds]="fileState.selectedFileIds()"
            (folderClicked)="navigateToFolder($event)"
            (fileClicked)="onFileClicked($event)"
            (folderAction)="onFolderAction($event)"
            (fileAction)="onFileAction($event)"
            (uploadClicked)="showUploadDialog.set(true)"
            (createFolderClicked)="showCreateFolderDialog.set(true)"
          />
        }
      </div>
    </div>

    <!-- Context panel template -->
    <ng-template #fileContextPanel>
      @if (contextPanelFileId()) {
        <app-file-context-panel
          [fileId]="contextPanelFileId()!"
          (moveRequested)="startMove($event, 'file')"
          (deleted)="onFileDeleted()"
          (renamed)="refreshContent()"
        />
      }
    </ng-template>

    <!-- Dialogs -->
    @if (showCreateFolderDialog()) {
      <app-create-folder-dialog
        (created)="createFolder($event)"
        (cancelled)="showCreateFolderDialog.set(false)"
      />
    }

    @if (showUploadDialog()) {
      <app-upload-dialog (closed)="showUploadDialog.set(false)" (fileUploaded)="refreshContent()" />
    }

    @if (showMoveDialog()) {
      <app-move-dialog
        [itemId]="moveItemId()!"
        [itemType]="moveItemType()!"
        (moved)="executeMove($event)"
        (cancelled)="showMoveDialog.set(false)"
      />
    }

    <!-- Tag manager dialog -->
    @if (showTagManager()) {
      <app-tag-manager-dialog
        (closed)="showTagManager.set(false)"
        (tagsChanged)="onTagsChanged()"
      />
    }

    <!-- Rename dialog (for folders) -->
    @if (showRenameDialog()) {
      <div
        class="fixed inset-0 bg-black/50 flex items-center justify-center z-50"
        (click)="showRenameDialog.set(false)"
      >
        <div
          class="bg-white rounded-xl shadow-xl w-full max-w-sm mx-4 p-6"
          (click)="$event.stopPropagation()"
        >
          <h2 class="text-lg font-semibold text-gray-900 mb-4" i18n="@@files.rename.title">
            Rename
          </h2>
          <input
            type="text"
            [(ngModel)]="renameValue"
            (keydown.enter)="executeRename()"
            class="w-full px-3 py-2 border border-gray-300 rounded-lg text-sm focus:outline-none focus:ring-2 focus:ring-blue-500 focus:border-blue-500"
            autofocus
          />
          <div class="flex justify-end gap-3 mt-4">
            <button
              (click)="showRenameDialog.set(false)"
              class="px-4 py-2 text-sm font-medium text-gray-700 bg-white border border-gray-300 rounded-lg hover:bg-gray-50 transition-colors"
              i18n="@@common.cancel"
            >
              Cancel
            </button>
            <button
              (click)="executeRename()"
              [disabled]="!renameValue.trim()"
              class="px-4 py-2 text-sm font-medium text-white bg-blue-600 rounded-lg hover:bg-blue-700 disabled:opacity-50 transition-colors"
              i18n="@@files.rename.save"
            >
              Save
            </button>
          </div>
        </div>
      </div>
    }
  `,
})
export class BrowsePageComponent implements OnInit, OnDestroy {
  private readonly fileService = inject(FileService);
  private readonly folderService = inject(FolderService);
  private readonly downloadService = inject(FileDownloadService);
  private readonly favoriteService = inject(FavoriteService);
  private readonly contextPanel = inject(ContextPanelService);
  readonly fileState = inject(FileStateService);
  private readonly sanitizer = inject(DomSanitizer);

  @ViewChild('fileContextPanel') fileContextPanelTemplate!: TemplateRef<unknown>;

  readonly icons = {
    UPLOAD: this.sanitizer.bypassSecurityTrustHtml(ICONS.UPLOAD),
    PLUS: this.sanitizer.bypassSecurityTrustHtml(ICONS.PLUS),
    GRID_VIEW: this.sanitizer.bypassSecurityTrustHtml(ICONS.GRID_VIEW),
    LIST_VIEW: this.sanitizer.bypassSecurityTrustHtml(ICONS.LIST_VIEW),
    CHEVRON_DOWN: this.sanitizer.bypassSecurityTrustHtml(ICONS.CHEVRON_DOWN),
    TAG: this.sanitizer.bypassSecurityTrustHtml(ICONS.TAG),
    STAR: this.sanitizer.bypassSecurityTrustHtml(ICONS.STAR),
    STAR_FILLED: this.sanitizer.bypassSecurityTrustHtml(ICONS.STAR_FILLED),
  };

  readonly gridViewLabel = $localize`:@@files.toolbar.gridView:Grid view`;
  readonly listViewLabel = $localize`:@@files.toolbar.listView:List view`;
  readonly manageTagsLabel = $localize`:@@files.toolbar.manageTags:Manage Tags`;
  readonly showFavoritesLabel = $localize`:@@files.toolbar.showFavorites:Show favorites`;
  readonly showAllLabel = $localize`:@@files.toolbar.showAll:Show all files`;

  readonly sortOptions = [
    { field: 'name' as const, label: $localize`:@@files.sort.name:Name` },
    { field: 'size' as const, label: $localize`:@@files.sort.size:Size` },
    { field: 'createdAt' as const, label: $localize`:@@files.sort.created:Created` },
    { field: 'updatedAt' as const, label: $localize`:@@files.sort.modified:Modified` },
  ];

  readonly folders = signal<FolderDto[]>([]);
  readonly files = signal<StoredFileDto[]>([]);
  readonly breadcrumbs = signal<FolderDto[]>([]);
  readonly loading = signal(false);

  readonly showCreateFolderDialog = signal(false);
  readonly showUploadDialog = signal(false);
  readonly showMoveDialog = signal(false);
  readonly showSortMenu = signal(false);
  readonly showRenameDialog = signal(false);
  readonly showTagManager = signal(false);
  readonly showFavoritesOnly = signal(false);
  private allFiles = signal<StoredFileDto[]>([]);

  readonly contextPanelFileId = signal<string | null>(null);
  readonly moveItemId = signal<string | null>(null);
  readonly moveItemType = signal<'file' | 'folder' | null>(null);

  renameValue = '';
  private renameItemId = '';
  private renameItemType: 'file' | 'folder' = 'folder';

  toggleSortMenu(): void {
    this.showSortMenu.update((v) => !v);
  }

  toggleShowFavorites(): void {
    this.showFavoritesOnly.update((v) => !v);
    if (this.showFavoritesOnly()) {
      this.favoriteService.getFavorites().subscribe((favFiles) => {
        this.allFiles.set(this.files());
        this.files.set(favFiles);
      });
    } else {
      this.files.set(this.allFiles());
    }
  }

  onTagsChanged(): void {
    // Tags were created or deleted — refresh is only needed if we show tags elsewhere
  }

  ngOnInit(): void {
    this.navigateToFolder(this.fileState.currentFolderId());
  }

  ngOnDestroy(): void {
    this.contextPanel.close();
  }

  navigateToFolder(folderId: string | null): void {
    this.fileState.navigateToFolder(folderId);
    this.contextPanel.close();
    this.refreshContent();

    if (folderId) {
      this.folderService.getBreadcrumb(folderId).subscribe((b) => this.breadcrumbs.set(b));
    } else {
      this.breadcrumbs.set([]);
    }
  }

  refreshContent(): void {
    this.loading.set(true);
    const folderId = this.fileState.currentFolderId() ?? '00000000-0000-0000-0000-000000000000';

    this.folderService.getFolders(folderId).subscribe((f) => this.folders.set(f));
    this.fileService.getFiles(folderId).subscribe((f) => {
      this.files.set(f);
      this.loading.set(false);
    });
  }

  onFileClicked(fileId: string): void {
    this.fileState.selectFile(fileId);
    this.contextPanelFileId.set(fileId);
    // Defer to next tick so the template ref is available
    setTimeout(() => {
      if (this.fileContextPanelTemplate) {
        this.contextPanel.open(this.fileContextPanelTemplate, fileId);
      }
    });
  }

  onFolderAction(action: FolderAction): void {
    switch (action.action) {
      case 'rename':
        this.startRename(action.folderId, 'folder');
        break;
      case 'move':
        this.startMove(action.folderId, 'folder');
        break;
      case 'delete':
        this.deleteFolder(action.folderId);
        break;
    }
  }

  onFileAction(action: FileAction): void {
    switch (action.action) {
      case 'download':
        this.downloadFile(action.fileId);
        break;
      case 'rename':
        this.startRename(action.fileId, 'file');
        break;
      case 'move':
        this.startMove(action.fileId, 'file');
        break;
      case 'delete':
        this.deleteFile(action.fileId);
        break;
    }
  }

  sortBy(field: 'name' | 'size' | 'createdAt' | 'updatedAt'): void {
    this.fileState.setSort(field);
    this.showSortMenu.set(false);
    this.sortCurrentContent();
  }

  createFolder(name: string): void {
    this.showCreateFolderDialog.set(false);
    const parentFolderId = this.fileState.currentFolderId() ?? undefined;
    this.folderService.createFolder({ name, parentFolderId }).subscribe((f) => {
      if (f) this.refreshContent();
    });
  }

  startMove(itemId: string, type: 'file' | 'folder'): void {
    this.moveItemId.set(itemId);
    this.moveItemType.set(type);
    this.showMoveDialog.set(true);
  }

  executeMove(targetFolderId: string | null): void {
    this.showMoveDialog.set(false);
    const itemId = this.moveItemId();
    const type = this.moveItemType();
    if (!itemId || !type) return;

    const targetId = targetFolderId ?? '00000000-0000-0000-0000-000000000000';

    if (type === 'file') {
      this.fileService.moveFile({ fileId: itemId, targetFolderId: targetId }).subscribe(() => {
        this.refreshContent();
        this.contextPanel.close();
      });
    } else {
      this.folderService
        .moveFolder({ folderId: itemId, targetParentFolderId: targetId })
        .subscribe(() => this.refreshContent());
    }
  }

  startRename(itemId: string, type: 'file' | 'folder'): void {
    this.renameItemId = itemId;
    this.renameItemType = type;

    if (type === 'folder') {
      const folder = this.folders().find((f) => f.id === itemId);
      this.renameValue = folder?.name ?? '';
    } else {
      const file = this.files().find((f) => f.id === itemId);
      this.renameValue = file?.name ?? '';
    }
    this.showRenameDialog.set(true);
  }

  executeRename(): void {
    const name = this.renameValue.trim();
    if (!name) return;

    this.showRenameDialog.set(false);

    if (this.renameItemType === 'folder') {
      this.folderService
        .renameFolder({ folderId: this.renameItemId, newName: name })
        .subscribe(() => this.refreshContent());
    } else {
      this.fileService
        .renameFile({ fileId: this.renameItemId, newName: name })
        .subscribe(() => this.refreshContent());
    }
  }

  onFileDeleted(): void {
    this.contextPanel.close();
    this.refreshContent();
  }

  private downloadFile(fileId: string): void {
    const file = this.files().find((f) => f.id === fileId);
    if (file) {
      this.downloadService.download(file.storageKey, file.name);
    }
  }

  private deleteFolder(folderId: string): void {
    this.folderService.deleteFolder(folderId).subscribe((ok) => {
      if (ok) this.refreshContent();
    });
  }

  private deleteFile(fileId: string): void {
    this.fileService.deleteFile(fileId).subscribe((ok) => {
      if (ok) this.refreshContent();
    });
  }

  private sortCurrentContent(): void {
    const field = this.fileState.sortBy();
    const dir = this.fileState.sortDirection();
    const multiplier = dir === 'asc' ? 1 : -1;

    this.files.update((files) =>
      [...files].sort((a, b) => {
        switch (field) {
          case 'name':
            return multiplier * a.name.localeCompare(b.name);
          case 'size':
            return multiplier * (a.size - b.size);
          case 'createdAt':
            return multiplier * (new Date(a.createdAt).getTime() - new Date(b.createdAt).getTime());
          case 'updatedAt':
            return multiplier * (new Date(a.updatedAt).getTime() - new Date(b.updatedAt).getTime());
        }
      }),
    );

    this.folders.update((folders) =>
      [...folders].sort((a, b) => multiplier * a.name.localeCompare(b.name)),
    );
  }
}
