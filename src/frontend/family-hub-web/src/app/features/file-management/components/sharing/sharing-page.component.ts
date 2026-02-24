import { Component, inject, signal, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { DomSanitizer, SafeHtml } from '@angular/platform-browser';
import { ICONS } from '../../../../shared/icons/icons';
import { SharingService } from '../../services/sharing.service';
import { ShareLinkDto, ShareLinkAccessLogDto } from '../../models/sharing.models';

@Component({
  selector: 'app-sharing-page',
  standalone: true,
  imports: [CommonModule, FormsModule],
  template: `
    <div class="h-full flex flex-col">
      <!-- Header -->
      <div class="flex items-center justify-between px-6 py-4 border-b border-gray-200 bg-white">
        <h2 class="text-lg font-semibold text-gray-900" i18n="@@files.sharing.title">
          Shared Links
        </h2>
      </div>

      <!-- Content -->
      <div class="flex-1 overflow-y-auto">
        @if (loading()) {
          <div class="flex items-center justify-center py-16">
            <div
              class="animate-spin rounded-full h-8 w-8 border-2 border-blue-600 border-t-transparent"
            ></div>
          </div>
        } @else if (shareLinks().length === 0) {
          <div class="flex flex-col items-center justify-center py-16 text-gray-400">
            <span [innerHTML]="shareIconLg" class="mb-3"></span>
            <p class="text-lg font-medium" i18n="@@files.sharing.empty">No shared links</p>
            <p class="text-sm mt-1" i18n="@@files.sharing.emptyDesc">
              Share files or folders from the file browser to create links.
            </p>
          </div>
        } @else {
          <div class="divide-y divide-gray-100">
            @for (link of shareLinks(); track link.id) {
              <div class="px-6 py-4">
                <div class="flex items-center justify-between">
                  <div class="flex items-center gap-3 min-w-0">
                    <span
                      [innerHTML]="link.resourceType === 'File' ? documentIcon : folderIcon"
                      class="text-gray-400 flex-shrink-0"
                    ></span>
                    <div class="min-w-0">
                      <div class="flex items-center gap-2">
                        <span [innerHTML]="linkIcon" class="text-gray-400"></span>
                        <code class="text-xs text-gray-600 font-mono truncate">
                          {{ link.token }}
                        </code>
                        <button
                          (click)="copyToken(link.token)"
                          class="text-xs text-blue-600 hover:text-blue-700 font-medium"
                          i18n="@@files.sharing.copy"
                        >
                          Copy
                        </button>
                      </div>
                      <div class="flex items-center gap-3 mt-1 text-xs text-gray-500">
                        <span>{{ link.resourceType }}</span>
                        <span>·</span>
                        <span>
                          {{ link.downloadCount }}
                          <span i18n="@@files.sharing.downloads">downloads</span>
                        </span>
                        @if (link.maxDownloads) {
                          <span>/ {{ link.maxDownloads }} max</span>
                        }
                        @if (link.expiresAt) {
                          <span>·</span>
                          <span i18n="@@files.sharing.expires">Expires</span>
                          {{ link.expiresAt | date: 'mediumDate' }}
                        }
                        <span>·</span>
                        <span>{{ link.createdAt | date: 'mediumDate' }}</span>
                      </div>
                    </div>
                  </div>
                  <div class="flex items-center gap-2">
                    <!-- Status -->
                    @if (link.isRevoked) {
                      <span
                        class="px-2 py-0.5 text-xs font-medium text-red-700 bg-red-100 rounded-full"
                        i18n="@@files.sharing.revoked"
                      >
                        Revoked
                      </span>
                    } @else if (link.isExpired) {
                      <span
                        class="px-2 py-0.5 text-xs font-medium text-yellow-700 bg-yellow-100 rounded-full"
                        i18n="@@files.sharing.expired"
                      >
                        Expired
                      </span>
                    } @else {
                      <span
                        class="px-2 py-0.5 text-xs font-medium text-green-700 bg-green-100 rounded-full"
                        i18n="@@files.sharing.active"
                      >
                        Active
                      </span>
                    }
                    @if (link.hasPassword) {
                      <span class="text-xs text-gray-400" title="Password protected">🔒</span>
                    }
                    <!-- Actions -->
                    <button
                      (click)="toggleAccessLog(link)"
                      class="text-xs text-blue-600 hover:text-blue-700 font-medium"
                      i18n="@@files.sharing.viewLog"
                    >
                      Log
                    </button>
                    @if (link.isAccessible) {
                      <button
                        (click)="revokeLink(link)"
                        class="text-xs text-red-500 hover:text-red-700 font-medium"
                        i18n="@@files.sharing.revoke"
                      >
                        Revoke
                      </button>
                    }
                  </div>
                </div>

                <!-- Access log (expandable) -->
                @if (expandedLinkId() === link.id) {
                  <div class="mt-3 ml-8 border-l-2 border-gray-200 pl-4">
                    @if (accessLog().length === 0) {
                      <p class="text-xs text-gray-400 py-2" i18n="@@files.sharing.noAccess">
                        No access recorded yet.
                      </p>
                    } @else {
                      <div class="space-y-1">
                        @for (entry of accessLog(); track entry.id) {
                          <div class="flex items-center gap-3 text-xs text-gray-500 py-1">
                            <span class="font-mono">{{ entry.ipAddress }}</span>
                            <span
                              class="px-1.5 py-0.5 rounded text-xs"
                              [class.bg-blue-100]="entry.action === 'View'"
                              [class.text-blue-700]="entry.action === 'View'"
                              [class.bg-green-100]="entry.action === 'Download'"
                              [class.text-green-700]="entry.action === 'Download'"
                            >
                              {{ entry.action }}
                            </span>
                            <span>{{ entry.accessedAt | date: 'medium' }}</span>
                          </div>
                        }
                      </div>
                    }
                  </div>
                }
              </div>
            }
          </div>
        }
      </div>
    </div>
  `,
})
export class SharingPageComponent implements OnInit {
  private readonly sharingService = inject(SharingService);
  private readonly sanitizer = inject(DomSanitizer);

  readonly shareLinks = signal<ShareLinkDto[]>([]);
  readonly accessLog = signal<ShareLinkAccessLogDto[]>([]);
  readonly loading = signal(true);
  readonly expandedLinkId = signal<string | null>(null);

  readonly shareIconLg: SafeHtml;
  readonly linkIcon: SafeHtml;
  readonly documentIcon: SafeHtml;
  readonly folderIcon: SafeHtml;

  constructor() {
    this.linkIcon = this.sanitizer.bypassSecurityTrustHtml(ICONS.LINK);
    this.documentIcon = this.sanitizer.bypassSecurityTrustHtml(ICONS.DOCUMENT);
    this.folderIcon = this.sanitizer.bypassSecurityTrustHtml(ICONS.FOLDER);
    const lgShare = ICONS.SHARE.replace('h-5 w-5', 'h-12 w-12');
    this.shareIconLg = this.sanitizer.bypassSecurityTrustHtml(lgShare);
  }

  ngOnInit(): void {
    this.loadShareLinks();
  }

  copyToken(token: string): void {
    navigator.clipboard.writeText(token);
  }

  toggleAccessLog(link: ShareLinkDto): void {
    if (this.expandedLinkId() === link.id) {
      this.expandedLinkId.set(null);
      return;
    }
    this.expandedLinkId.set(link.id);
    this.sharingService.getAccessLog(link.id, link.familyId).subscribe((log) => {
      this.accessLog.set(log);
    });
  }

  revokeLink(link: ShareLinkDto): void {
    this.sharingService.revokeShareLink(link.id, link.familyId).subscribe((ok) => {
      if (ok) {
        this.loadShareLinks();
      }
    });
  }

  private loadShareLinks(): void {
    // TODO: Get familyId from user context / auth service
    // For now we load without familyId filter — backend uses JWT context
    this.sharingService.getShareLinks('00000000-0000-0000-0000-000000000000').subscribe((links) => {
      this.shareLinks.set(links);
      this.loading.set(false);
    });
  }
}
