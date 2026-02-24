import { Component, inject, signal, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { DomSanitizer, SafeHtml } from '@angular/platform-browser';
import { ICONS } from '../../../../shared/icons/icons';
import { SecureNoteService } from '../../services/secure-note.service';
import { SecureNoteDto, DecryptedNote } from '../../models/secure-note.models';

@Component({
  selector: 'app-notes-page',
  standalone: true,
  imports: [CommonModule, FormsModule],
  template: `
    <div class="h-full flex">
      <!-- Notes sidebar -->
      <div class="w-72 border-r border-gray-200 bg-white flex flex-col">
        <div class="flex items-center justify-between px-4 py-3 border-b border-gray-200">
          <h2 class="text-sm font-semibold text-gray-900" i18n="@@files.notes.title">Notes</h2>
          <button
            (click)="createNote()"
            class="p-1.5 rounded-lg hover:bg-gray-100 text-gray-500 transition-colors"
          >
            <span [innerHTML]="plusIcon"></span>
          </button>
        </div>

        <!-- Category filter -->
        <div class="px-4 py-2 border-b border-gray-100">
          <select
            [(ngModel)]="categoryFilter"
            (ngModelChange)="onCategoryChange()"
            class="w-full px-2 py-1.5 text-xs border border-gray-200 rounded-lg focus:outline-none focus:ring-2 focus:ring-blue-500"
          >
            <option value="" i18n="@@files.notes.allCategories">All Categories</option>
            <option value="personal" i18n="@@files.notes.catPersonal">Personal</option>
            <option value="medical" i18n="@@files.notes.catMedical">Medical</option>
            <option value="financial" i18n="@@files.notes.catFinancial">Financial</option>
            <option value="legal" i18n="@@files.notes.catLegal">Legal</option>
            <option value="other" i18n="@@files.notes.catOther">Other</option>
          </select>
        </div>

        <!-- Notes list -->
        <div class="flex-1 overflow-y-auto">
          @if (loading()) {
            <div class="flex items-center justify-center py-8">
              <div
                class="animate-spin rounded-full h-6 w-6 border-2 border-blue-600 border-t-transparent"
              ></div>
            </div>
          } @else if (notes().length === 0) {
            <div class="flex flex-col items-center justify-center py-8 px-4 text-gray-400">
              <span [innerHTML]="lockIconLg" class="mb-2"></span>
              <p class="text-sm font-medium text-center" i18n="@@files.notes.noNotes">
                No notes yet
              </p>
              <p class="text-xs mt-1 text-center" i18n="@@files.notes.noNotesDesc">
                Create a secure note to get started.
              </p>
            </div>
          } @else {
            @for (note of notes(); track note.id) {
              <button
                (click)="selectNote(note)"
                class="w-full text-left px-4 py-3 border-b border-gray-50 hover:bg-gray-50 transition-colors"
                [class.bg-blue-50]="selectedNote()?.id === note.id"
                [class.border-l-2]="selectedNote()?.id === note.id"
                [class.border-l-blue-600]="selectedNote()?.id === note.id"
              >
                <div class="flex items-center gap-2">
                  <span [innerHTML]="lockIcon" class="text-gray-400 flex-shrink-0"></span>
                  <div class="min-w-0 flex-1">
                    <p class="text-sm font-medium text-gray-900 truncate">
                      {{ note.title || untitledLabel }}
                    </p>
                    <div class="flex items-center gap-2 mt-0.5">
                      <span class="text-xs px-1.5 py-0.5 rounded bg-gray-100 text-gray-600">
                        {{ note.category }}
                      </span>
                      <span class="text-xs text-gray-400">
                        {{ note.updatedAt | date: 'shortDate' }}
                      </span>
                    </div>
                  </div>
                </div>
              </button>
            }
          }
        </div>
      </div>

      <!-- Note editor -->
      <div class="flex-1 flex flex-col bg-white">
        @if (selectedNote()) {
          <!-- Editor header -->
          <div class="flex items-center justify-between px-6 py-3 border-b border-gray-200">
            <div class="flex items-center gap-3 flex-1 min-w-0">
              <input
                [(ngModel)]="editTitle"
                (ngModelChange)="markDirty()"
                class="flex-1 text-lg font-semibold text-gray-900 bg-transparent border-none focus:outline-none"
                [placeholder]="titlePlaceholder"
              />
              <select
                [(ngModel)]="editCategory"
                (ngModelChange)="markDirty()"
                class="text-xs px-2 py-1 border border-gray-200 rounded-lg focus:outline-none focus:ring-2 focus:ring-blue-500"
              >
                <option value="personal">Personal</option>
                <option value="medical">Medical</option>
                <option value="financial">Financial</option>
                <option value="legal">Legal</option>
                <option value="other">Other</option>
              </select>
            </div>
            <div class="flex items-center gap-2">
              @if (isDirty()) {
                <button
                  (click)="saveNote()"
                  [disabled]="saving()"
                  class="flex items-center gap-1.5 px-3 py-1.5 text-sm font-medium text-white bg-blue-600 rounded-lg hover:bg-blue-700 disabled:opacity-50 transition-colors"
                >
                  @if (saving()) {
                    <span i18n="@@common.processing">Processing...</span>
                  } @else {
                    <span i18n="@@files.notes.save">Save</span>
                  }
                </button>
              }
              @if (justSaved()) {
                <span class="text-xs text-green-600" i18n="@@files.notes.saved">Saved!</span>
              }
              <button
                (click)="confirmDelete()"
                class="p-1.5 rounded-lg hover:bg-red-50 text-gray-400 hover:text-red-600 transition-colors"
              >
                <span [innerHTML]="trashIcon"></span>
              </button>
            </div>
          </div>

          <!-- Editor body -->
          <div class="flex-1 p-6">
            <textarea
              [(ngModel)]="editContent"
              (ngModelChange)="markDirty()"
              class="w-full h-full resize-none text-sm text-gray-700 bg-transparent border-none focus:outline-none leading-relaxed"
              [placeholder]="contentPlaceholder"
            ></textarea>
          </div>

          <!-- Footer with metadata -->
          <div
            class="px-6 py-2 border-t border-gray-100 text-xs text-gray-400 flex items-center gap-4"
          >
            <span>
              <span i18n="@@files.notes.created">Created</span>
              {{ selectedNote()!.createdAt | date: 'medium' }}
            </span>
            <span>
              <span i18n="@@files.notes.modified">Modified</span>
              {{ selectedNote()!.updatedAt | date: 'medium' }}
            </span>
          </div>
        } @else {
          <div class="flex-1 flex flex-col items-center justify-center text-gray-400">
            <span [innerHTML]="lockIconLg" class="mb-3"></span>
            <p class="text-sm font-medium" i18n="@@files.notes.selectNote">
              Select a note or create a new one
            </p>
          </div>
        }
      </div>

      <!-- Delete confirmation dialog -->
      @if (showDeleteConfirm()) {
        <div class="fixed inset-0 bg-black/50 flex items-center justify-center z-50">
          <div class="bg-white rounded-xl shadow-xl w-full max-w-sm mx-4 p-6">
            <p class="text-sm text-gray-700 mb-4" i18n="@@files.notes.deleteConfirm">
              Are you sure you want to delete this note? This cannot be undone.
            </p>
            <div class="flex justify-end gap-3">
              <button
                (click)="showDeleteConfirm.set(false)"
                class="px-4 py-2 text-sm font-medium text-gray-700 bg-white border border-gray-300 rounded-lg hover:bg-gray-50"
                i18n="@@common.cancel"
              >
                Cancel
              </button>
              <button
                (click)="doDelete()"
                class="px-4 py-2 text-sm font-medium text-white bg-red-600 rounded-lg hover:bg-red-700"
                i18n="@@common.confirm"
              >
                Confirm
              </button>
            </div>
          </div>
        </div>
      }
    </div>
  `,
})
export class NotesPageComponent implements OnInit {
  private readonly noteService = inject(SecureNoteService);
  private readonly sanitizer = inject(DomSanitizer);

  readonly notes = signal<DecryptedNote[]>([]);
  readonly selectedNote = signal<DecryptedNote | null>(null);
  readonly loading = signal(true);
  readonly saving = signal(false);
  readonly isDirty = signal(false);
  readonly justSaved = signal(false);
  readonly showDeleteConfirm = signal(false);

  categoryFilter = '';
  editTitle = '';
  editContent = '';
  editCategory = 'personal';

  readonly untitledLabel = $localize`:@@files.notes.untitled:Untitled`;
  readonly titlePlaceholder = $localize`:@@files.notes.titlePlaceholder:Note title`;
  readonly contentPlaceholder = $localize`:@@files.notes.contentPlaceholder:Start writing...`;

  // Icons
  readonly plusIcon: SafeHtml;
  readonly lockIcon: SafeHtml;
  readonly lockIconLg: SafeHtml;
  readonly trashIcon: SafeHtml;

  constructor() {
    this.plusIcon = this.sanitizer.bypassSecurityTrustHtml(ICONS.PLUS);
    this.lockIcon = this.sanitizer.bypassSecurityTrustHtml(ICONS.LOCK);
    this.trashIcon = this.sanitizer.bypassSecurityTrustHtml(ICONS.TRASH);
    const lgLock = ICONS.LOCK.replace('h-5 w-5', 'h-12 w-12');
    this.lockIconLg = this.sanitizer.bypassSecurityTrustHtml(lgLock);
  }

  ngOnInit(): void {
    this.loadNotes();
  }

  onCategoryChange(): void {
    this.loadNotes();
  }

  selectNote(note: DecryptedNote): void {
    this.selectedNote.set(note);
    this.editTitle = note.title;
    this.editContent = note.content;
    this.editCategory = note.category;
    this.isDirty.set(false);
    this.justSaved.set(false);
  }

  createNote(): void {
    // For the MVP, we store notes as plaintext (encryption keys managed client-side in a later iteration).
    // The backend expects encrypted fields — we send the plaintext as base64 for now.
    const input = {
      category: 'personal',
      encryptedTitle: btoa(''),
      encryptedContent: btoa(''),
      iv: btoa('0000000000000000'),
      salt: btoa('0000000000000000'),
      sentinel: btoa('sentinel-check'),
    };
    this.noteService.createNote(input).subscribe((noteId) => {
      if (noteId) {
        this.loadNotes();
      }
    });
  }

  markDirty(): void {
    this.isDirty.set(true);
    this.justSaved.set(false);
  }

  saveNote(): void {
    const note = this.selectedNote();
    if (!note) return;

    this.saving.set(true);
    this.noteService
      .updateNote({
        noteId: note.id,
        category: this.editCategory,
        encryptedTitle: btoa(this.editTitle),
        encryptedContent: btoa(this.editContent),
        iv: btoa('0000000000000000'),
      })
      .subscribe((ok) => {
        this.saving.set(false);
        if (ok) {
          this.isDirty.set(false);
          this.justSaved.set(true);
          // Update local state
          this.notes.update((notes) =>
            notes.map((n) =>
              n.id === note.id
                ? {
                    ...n,
                    title: this.editTitle,
                    content: this.editContent,
                    category: this.editCategory,
                  }
                : n,
            ),
          );
          this.selectedNote.update((n) =>
            n
              ? {
                  ...n,
                  title: this.editTitle,
                  content: this.editContent,
                  category: this.editCategory,
                }
              : n,
          );
        }
      });
  }

  confirmDelete(): void {
    this.showDeleteConfirm.set(true);
  }

  doDelete(): void {
    const note = this.selectedNote();
    if (!note) return;

    this.noteService.deleteNote(note.id).subscribe((ok) => {
      if (ok) {
        this.notes.update((notes) => notes.filter((n) => n.id !== note.id));
        this.selectedNote.set(null);
        this.editTitle = '';
        this.editContent = '';
      }
      this.showDeleteConfirm.set(false);
    });
  }

  private loadNotes(): void {
    const category = this.categoryFilter || undefined;
    this.noteService.getNotes(category).subscribe((rawNotes) => {
      // Decrypt notes (MVP: base64-encoded plaintext)
      const decrypted: DecryptedNote[] = rawNotes.map((n) => this.decryptNote(n));
      this.notes.set(decrypted);
      this.loading.set(false);
    });
  }

  private decryptNote(note: SecureNoteDto): DecryptedNote {
    try {
      return {
        id: note.id,
        category: note.category,
        title: atob(note.encryptedTitle),
        content: atob(note.encryptedContent),
        createdAt: note.createdAt,
        updatedAt: note.updatedAt,
      };
    } catch {
      return {
        id: note.id,
        category: note.category,
        title: note.encryptedTitle,
        content: note.encryptedContent,
        createdAt: note.createdAt,
        updatedAt: note.updatedAt,
      };
    }
  }
}
