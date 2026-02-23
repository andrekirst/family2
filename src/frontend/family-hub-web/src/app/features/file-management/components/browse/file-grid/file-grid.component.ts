import { Component, input, output } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FolderDto } from '../../../models/folder.models';
import { StoredFileDto } from '../../../models/file.models';
import { ViewMode } from '../../../services/file-state.service';
import { FolderItemComponent, FolderAction } from '../folder-item/folder-item.component';
import { FileGridItemComponent, FileAction } from '../file-grid-item/file-grid-item.component';
import { FileListItemComponent } from '../file-list-item/file-list-item.component';
import { EmptyStateComponent } from '../empty-state/empty-state.component';

@Component({
  selector: 'app-file-grid',
  standalone: true,
  imports: [
    CommonModule,
    FolderItemComponent,
    FileGridItemComponent,
    FileListItemComponent,
    EmptyStateComponent,
  ],
  template: `
    @if (folders().length === 0 && files().length === 0) {
      <app-empty-state
        (uploadClicked)="uploadClicked.emit()"
        (createFolderClicked)="createFolderClicked.emit()"
      />
    } @else {
      @if (viewMode() === 'grid') {
        <!-- Grid View -->
        <div
          class="grid grid-cols-2 sm:grid-cols-3 md:grid-cols-4 lg:grid-cols-5 xl:grid-cols-6 gap-3 p-4"
        >
          @for (folder of folders(); track folder.id) {
            <app-folder-item
              [folder]="folder"
              (clicked)="folderClicked.emit($event)"
              (actionTriggered)="folderAction.emit($event)"
            />
          }
          @for (file of files(); track file.id) {
            <app-file-grid-item
              [file]="file"
              [selected]="selectedFileIds().has(file.id)"
              (clicked)="fileClicked.emit($event)"
              (actionTriggered)="fileAction.emit($event)"
            />
          }
        </div>
      } @else {
        <!-- List View -->
        <div class="flex flex-col">
          <!-- List header -->
          <div
            class="flex items-center gap-4 px-4 py-2 text-xs font-medium text-gray-500 uppercase tracking-wider border-b border-gray-200 bg-gray-50"
          >
            <span class="w-5"></span>
            <span class="flex-1" i18n="@@files.list.name">Name</span>
            <span class="w-20 text-right" i18n="@@files.list.size">Size</span>
            <span class="w-32 text-right" i18n="@@files.list.modified">Modified</span>
            <span class="w-8"></span>
          </div>
          @for (folder of folders(); track folder.id) {
            <app-folder-item
              [folder]="folder"
              (clicked)="folderClicked.emit($event)"
              (actionTriggered)="folderAction.emit($event)"
            />
          }
          @for (file of files(); track file.id) {
            <app-file-list-item
              [file]="file"
              [selected]="selectedFileIds().has(file.id)"
              (clicked)="fileClicked.emit($event)"
              (actionTriggered)="fileAction.emit($event)"
            />
          }
        </div>
      }
    }
  `,
})
export class FileGridComponent {
  readonly folders = input<FolderDto[]>([]);
  readonly files = input<StoredFileDto[]>([]);
  readonly viewMode = input<ViewMode>('grid');
  readonly selectedFileIds = input<Set<string>>(new Set());

  readonly folderClicked = output<string>();
  readonly fileClicked = output<string>();
  readonly folderAction = output<FolderAction>();
  readonly fileAction = output<FileAction>();
  readonly uploadClicked = output<void>();
  readonly createFolderClicked = output<void>();
}
