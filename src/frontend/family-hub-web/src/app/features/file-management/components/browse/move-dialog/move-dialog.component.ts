import { Component, inject, input, output, signal, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { DomSanitizer, SafeHtml } from '@angular/platform-browser';
import { ICONS } from '../../../../../shared/icons/icons';
import { FolderService } from '../../../services/folder.service';
import { FolderDto } from '../../../models/folder.models';

interface FolderNode {
  folder: FolderDto;
  children: FolderNode[];
  expanded: boolean;
  loaded: boolean;
}

@Component({
  selector: 'app-move-dialog',
  standalone: true,
  imports: [CommonModule],
  template: `
    <div
      class="fixed inset-0 bg-black/50 flex items-center justify-center z-50"
      (click)="cancelled.emit()"
    >
      <div
        class="bg-white rounded-xl shadow-xl w-full max-w-md mx-4 p-6"
        (click)="$event.stopPropagation()"
      >
        <h2 class="text-lg font-semibold text-gray-900 mb-4" i18n="@@files.move.title">
          Move to...
        </h2>

        <div class="border border-gray-200 rounded-lg max-h-64 overflow-y-auto p-2">
          <!-- Root -->
          <button
            (click)="selectedFolderId.set(null)"
            class="flex items-center gap-2 w-full px-3 py-2 text-sm rounded-lg transition-colors"
            [class.bg-blue-50]="selectedFolderId() === null"
            [class.text-blue-700]="selectedFolderId() === null"
            [class.text-gray-700]="selectedFolderId() !== null"
            [class.hover:bg-gray-100]="selectedFolderId() !== null"
          >
            <span [innerHTML]="folderIcon" class="flex-shrink-0"></span>
            <span i18n="@@files.move.root">Root</span>
          </button>

          @for (node of rootNodes(); track node.folder.id) {
            <ng-container
              *ngTemplateOutlet="folderTree; context: { $implicit: node, depth: 1 }"
            ></ng-container>
          }
        </div>

        <div class="flex justify-end gap-3 mt-4">
          <button
            (click)="cancelled.emit()"
            class="px-4 py-2 text-sm font-medium text-gray-700 bg-white border border-gray-300 rounded-lg hover:bg-gray-50 transition-colors"
            i18n="@@common.cancel"
          >
            Cancel
          </button>
          <button
            (click)="confirm()"
            class="px-4 py-2 text-sm font-medium text-white bg-blue-600 rounded-lg hover:bg-blue-700 transition-colors"
            i18n="@@files.move.confirm"
          >
            Move Here
          </button>
        </div>
      </div>
    </div>

    <ng-template #folderTree let-node let-depth="depth">
      <div [style.padding-left.px]="depth * 16">
        <button
          (click)="selectFolder(node)"
          class="flex items-center gap-2 w-full px-3 py-2 text-sm rounded-lg transition-colors"
          [class.bg-blue-50]="selectedFolderId() === node.folder.id"
          [class.text-blue-700]="selectedFolderId() === node.folder.id"
          [class.text-gray-700]="selectedFolderId() !== node.folder.id"
          [class.hover:bg-gray-100]="selectedFolderId() !== node.folder.id"
        >
          <span [innerHTML]="folderIcon" class="flex-shrink-0"></span>
          <span class="truncate">{{ node.folder.name }}</span>
        </button>
      </div>
      @if (node.expanded) {
        @for (child of node.children; track child.folder.id) {
          <ng-container
            *ngTemplateOutlet="folderTree; context: { $implicit: child, depth: depth + 1 }"
          ></ng-container>
        }
      }
    </ng-template>
  `,
})
export class MoveDialogComponent implements OnInit {
  private readonly folderService = inject(FolderService);

  readonly itemId = input.required<string>();
  readonly itemType = input.required<'file' | 'folder'>();
  readonly moved = output<string | null>();
  readonly cancelled = output<void>();

  readonly rootNodes = signal<FolderNode[]>([]);
  readonly selectedFolderId = signal<string | null>(null);
  readonly folderIcon: SafeHtml;

  constructor(sanitizer: DomSanitizer) {
    this.folderIcon = sanitizer.bypassSecurityTrustHtml(ICONS.FOLDER);
  }

  ngOnInit(): void {
    this.loadChildren(null);
  }

  selectFolder(node: FolderNode): void {
    this.selectedFolderId.set(node.folder.id);
    if (!node.loaded) {
      this.loadChildren(node.folder.id, node);
    }
    node.expanded = !node.expanded;
    this.rootNodes.update((n) => [...n]);
  }

  confirm(): void {
    this.moved.emit(this.selectedFolderId());
  }

  private loadChildren(parentFolderId: string | null, parentNode?: FolderNode): void {
    // Use empty string for root folder queries
    const queryId = parentFolderId ?? '00000000-0000-0000-0000-000000000000';
    this.folderService.getFolders(queryId).subscribe((folders) => {
      const nodes: FolderNode[] = folders
        .filter((f) => f.id !== this.itemId())
        .map((f) => ({ folder: f, children: [], expanded: false, loaded: false }));

      if (parentNode) {
        parentNode.children = nodes;
        parentNode.loaded = true;
        this.rootNodes.update((n) => [...n]);
      } else {
        this.rootNodes.set(nodes);
      }
    });
  }
}
