import { Component, inject, signal, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { DomSanitizer, SafeHtml } from '@angular/platform-browser';
import { ICONS } from '../../../../shared/icons/icons';
import { InboxService } from '../../services/inbox.service';
import { OrganizationRuleDto, ProcessingLogEntryDto } from '../../models/inbox.models';

@Component({
  selector: 'app-inbox-page',
  standalone: true,
  imports: [CommonModule, FormsModule],
  template: `
    <div class="h-full flex flex-col">
      <!-- Header -->
      <div class="flex items-center justify-between px-6 py-4 border-b border-gray-200 bg-white">
        <h2 class="text-lg font-semibold text-gray-900" i18n="@@files.inbox.title">Inbox</h2>
        <div class="flex items-center gap-2">
          <button
            (click)="processInbox()"
            [disabled]="processing()"
            class="flex items-center gap-2 px-4 py-2 text-sm font-medium text-white bg-blue-600 rounded-lg hover:bg-blue-700 disabled:opacity-50 transition-colors"
          >
            <span [innerHTML]="playIcon"></span>
            @if (processing()) {
              <span i18n="@@files.inbox.processing">Processing...</span>
            } @else {
              <span i18n="@@files.inbox.processFiles">Process Files</span>
            }
          </button>
          <button
            (click)="showRuleEditor.set(true)"
            class="flex items-center gap-2 px-4 py-2 text-sm font-medium text-gray-700 bg-white border border-gray-300 rounded-lg hover:bg-gray-50 transition-colors"
          >
            <span [innerHTML]="plusIcon"></span>
            <span i18n="@@files.inbox.newRule">New Rule</span>
          </button>
        </div>
      </div>

      <!-- Tab strip -->
      <div class="flex border-b border-gray-200 bg-white px-6">
        <button
          (click)="activeTab.set('rules')"
          class="px-4 py-3 text-sm font-medium border-b-2 transition-colors"
          [class.border-blue-600]="activeTab() === 'rules'"
          [class.text-blue-600]="activeTab() === 'rules'"
          [class.border-transparent]="activeTab() !== 'rules'"
          [class.text-gray-500]="activeTab() !== 'rules'"
          i18n="@@files.inbox.rulesTab"
        >
          Rules
        </button>
        <button
          (click)="activeTab.set('log')"
          class="px-4 py-3 text-sm font-medium border-b-2 transition-colors"
          [class.border-blue-600]="activeTab() === 'log'"
          [class.text-blue-600]="activeTab() === 'log'"
          [class.border-transparent]="activeTab() !== 'log'"
          [class.text-gray-500]="activeTab() !== 'log'"
          i18n="@@files.inbox.logTab"
        >
          Processing Log
        </button>
      </div>

      <!-- Content -->
      <div class="flex-1 overflow-y-auto">
        @if (loading()) {
          <div class="flex items-center justify-center py-16">
            <div
              class="animate-spin rounded-full h-8 w-8 border-2 border-blue-600 border-t-transparent"
            ></div>
          </div>
        } @else if (activeTab() === 'rules') {
          <!-- Rules list -->
          @if (rules().length === 0) {
            <div class="flex flex-col items-center justify-center py-16 text-gray-400">
              <span [innerHTML]="inboxIconLg" class="mb-3"></span>
              <p class="text-lg font-medium" i18n="@@files.inbox.noRules">No rules yet</p>
              <p class="text-sm mt-1" i18n="@@files.inbox.noRulesDesc">
                Create rules to auto-organize incoming files.
              </p>
            </div>
          } @else {
            <div class="divide-y divide-gray-100">
              @for (rule of rules(); track rule.id) {
                <div class="px-6 py-4 hover:bg-gray-50 transition-colors">
                  <div class="flex items-center justify-between">
                    <div class="flex items-center gap-3 min-w-0">
                      <button
                        (click)="toggleRule(rule)"
                        class="relative inline-flex h-5 w-9 flex-shrink-0 cursor-pointer rounded-full border-2 border-transparent transition-colors duration-200 ease-in-out"
                        [class.bg-blue-600]="rule.isEnabled"
                        [class.bg-gray-200]="!rule.isEnabled"
                      >
                        <span
                          class="inline-block h-4 w-4 transform rounded-full bg-white transition duration-200 ease-in-out"
                          [class.translate-x-4]="rule.isEnabled"
                          [class.translate-x-0]="!rule.isEnabled"
                        ></span>
                      </button>
                      <div class="min-w-0">
                        <p class="text-sm font-medium text-gray-900 truncate">{{ rule.name }}</p>
                        <div class="flex items-center gap-2 mt-0.5 text-xs text-gray-500">
                          <span class="px-1.5 py-0.5 bg-gray-100 rounded">
                            {{ rule.actionType }}
                          </span>
                          <span>·</span>
                          <span i18n="@@files.inbox.priority">Priority</span>
                          {{ rule.priority }}
                        </div>
                      </div>
                    </div>
                    <div class="flex items-center gap-2">
                      <button
                        (click)="editRule(rule)"
                        class="p-1.5 rounded-lg hover:bg-gray-100 text-gray-400 hover:text-gray-600 transition-colors"
                      >
                        <span [innerHTML]="pencilIcon"></span>
                      </button>
                      <button
                        (click)="deleteRule(rule)"
                        class="p-1.5 rounded-lg hover:bg-red-50 text-gray-400 hover:text-red-600 transition-colors"
                      >
                        <span [innerHTML]="trashIcon"></span>
                      </button>
                    </div>
                  </div>
                </div>
              }
            </div>
          }
        } @else {
          <!-- Processing log -->
          @if (logEntries().length === 0) {
            <div class="flex flex-col items-center justify-center py-16 text-gray-400">
              <span [innerHTML]="clockIconLg" class="mb-3"></span>
              <p class="text-lg font-medium" i18n="@@files.inbox.noLog">No processing history</p>
              <p class="text-sm mt-1" i18n="@@files.inbox.noLogDesc">
                Process inbox files to see results here.
              </p>
            </div>
          } @else {
            <div class="divide-y divide-gray-100">
              @for (entry of logEntries(); track entry.id) {
                <div class="px-6 py-3">
                  <div class="flex items-center justify-between">
                    <div class="flex items-center gap-3 min-w-0">
                      @if (entry.success) {
                        <span [innerHTML]="checkIcon" class="text-green-500 flex-shrink-0"></span>
                      } @else {
                        <span [innerHTML]="closeIcon" class="text-red-500 flex-shrink-0"></span>
                      }
                      <div class="min-w-0">
                        <p class="text-sm text-gray-900 truncate">{{ entry.fileName }}</p>
                        <div class="flex items-center gap-2 mt-0.5 text-xs text-gray-500">
                          @if (entry.matchedRuleName) {
                            <span class="px-1.5 py-0.5 bg-blue-50 text-blue-700 rounded">
                              {{ entry.matchedRuleName }}
                            </span>
                          }
                          @if (entry.actionTaken) {
                            <span>{{ entry.actionTaken }}</span>
                          }
                          @if (entry.appliedTagNames) {
                            <span>
                              <span i18n="@@files.inbox.tags">Tags:</span>
                              {{ entry.appliedTagNames }}
                            </span>
                          }
                          @if (entry.errorMessage) {
                            <span class="text-red-500">{{ entry.errorMessage }}</span>
                          }
                        </div>
                      </div>
                    </div>
                    <span class="text-xs text-gray-400 flex-shrink-0">
                      {{ entry.processedAt | date: 'medium' }}
                    </span>
                  </div>
                </div>
              }
            </div>
          }
        }
      </div>

      <!-- Process results banner -->
      @if (lastResult()) {
        <div
          class="px-6 py-3 border-t border-gray-200 bg-green-50 text-sm text-green-800 flex items-center justify-between"
        >
          <span>
            {{ lastResult()!.filesProcessed }}
            <span i18n="@@files.inbox.filesProcessed">files processed</span>,
            {{ lastResult()!.rulesMatched }}
            <span i18n="@@files.inbox.rulesMatched">rules matched</span>
          </span>
          <button (click)="lastResult.set(null)" class="text-green-600 hover:text-green-800">
            <span [innerHTML]="closeIcon"></span>
          </button>
        </div>
      }

      <!-- Rule editor dialog -->
      @if (showRuleEditor()) {
        <div class="fixed inset-0 bg-black/50 flex items-center justify-center z-50">
          <div class="bg-white rounded-xl shadow-xl w-full max-w-lg mx-4 p-6">
            <h3 class="text-lg font-semibold text-gray-900 mb-4">
              @if (editingRule()) {
                <span i18n="@@files.inbox.editRule">Edit Rule</span>
              } @else {
                <span i18n="@@files.inbox.newRule">New Rule</span>
              }
            </h3>

            <div class="space-y-4">
              <div>
                <label
                  class="block text-sm font-medium text-gray-700 mb-1"
                  i18n="@@files.inbox.ruleName"
                >
                  Rule Name
                </label>
                <input
                  [(ngModel)]="ruleName"
                  class="w-full px-3 py-2 border border-gray-300 rounded-lg text-sm focus:outline-none focus:ring-2 focus:ring-blue-500"
                  [placeholder]="ruleNamePlaceholder"
                />
              </div>

              <div>
                <label
                  class="block text-sm font-medium text-gray-700 mb-1"
                  i18n="@@files.inbox.conditionField"
                >
                  Condition Field
                </label>
                <select
                  [(ngModel)]="conditionField"
                  class="w-full px-3 py-2 border border-gray-300 rounded-lg text-sm focus:outline-none focus:ring-2 focus:ring-blue-500"
                >
                  <option value="extension">Extension</option>
                  <option value="mimeType">MIME Type</option>
                  <option value="name">File Name</option>
                  <option value="size">File Size</option>
                </select>
              </div>

              <div>
                <label
                  class="block text-sm font-medium text-gray-700 mb-1"
                  i18n="@@files.inbox.conditionValue"
                >
                  Condition Value
                </label>
                <input
                  [(ngModel)]="conditionValue"
                  class="w-full px-3 py-2 border border-gray-300 rounded-lg text-sm focus:outline-none focus:ring-2 focus:ring-blue-500"
                  [placeholder]="conditionValuePlaceholder"
                />
              </div>

              <div>
                <label
                  class="block text-sm font-medium text-gray-700 mb-1"
                  i18n="@@files.inbox.actionType"
                >
                  Action
                </label>
                <select
                  [(ngModel)]="actionType"
                  class="w-full px-3 py-2 border border-gray-300 rounded-lg text-sm focus:outline-none focus:ring-2 focus:ring-blue-500"
                >
                  <option value="MoveToFolder" i18n="@@files.inbox.actionMove">
                    Move to Folder
                  </option>
                  <option value="ApplyTag" i18n="@@files.inbox.actionTag">Apply Tag</option>
                  <option value="AddToAlbum" i18n="@@files.inbox.actionAlbum">Add to Album</option>
                </select>
              </div>

              <div>
                <label
                  class="block text-sm font-medium text-gray-700 mb-1"
                  i18n="@@files.inbox.actionTarget"
                >
                  Action Target
                </label>
                <input
                  [(ngModel)]="actionTarget"
                  class="w-full px-3 py-2 border border-gray-300 rounded-lg text-sm focus:outline-none focus:ring-2 focus:ring-blue-500"
                  [placeholder]="actionTargetPlaceholder"
                />
              </div>
            </div>

            <div class="flex justify-end gap-3 mt-6">
              <button
                (click)="cancelRuleEditor()"
                class="px-4 py-2 text-sm font-medium text-gray-700 bg-white border border-gray-300 rounded-lg hover:bg-gray-50"
                i18n="@@common.cancel"
              >
                Cancel
              </button>
              <button
                (click)="saveRule()"
                [disabled]="!ruleName.trim()"
                class="px-4 py-2 text-sm font-medium text-white bg-blue-600 rounded-lg hover:bg-blue-700 disabled:opacity-50"
              >
                @if (editingRule()) {
                  <span i18n="@@files.inbox.saveRule">Save</span>
                } @else {
                  <span i18n="@@files.inbox.createRule">Create Rule</span>
                }
              </button>
            </div>
          </div>
        </div>
      }
    </div>
  `,
})
export class InboxPageComponent implements OnInit {
  private readonly inboxService = inject(InboxService);
  private readonly sanitizer = inject(DomSanitizer);

  // TODO: Get familyId from user context / auth service
  private readonly familyId = '00000000-0000-0000-0000-000000000000';

  readonly rules = signal<OrganizationRuleDto[]>([]);
  readonly logEntries = signal<ProcessingLogEntryDto[]>([]);
  readonly loading = signal(true);
  readonly processing = signal(false);
  readonly activeTab = signal<'rules' | 'log'>('rules');
  readonly showRuleEditor = signal(false);
  readonly editingRule = signal<OrganizationRuleDto | null>(null);
  readonly lastResult = signal<{
    filesProcessed: number;
    rulesMatched: number;
  } | null>(null);

  // Rule editor form fields
  ruleName = '';
  conditionField = 'extension';
  conditionValue = '';
  actionType = 'MoveToFolder';
  actionTarget = '';

  readonly ruleNamePlaceholder = $localize`:@@files.inbox.ruleNamePlaceholder:e.g., Sort PDFs to Documents`;
  readonly conditionValuePlaceholder = $localize`:@@files.inbox.conditionValuePlaceholder:e.g., .pdf, image/*`;
  readonly actionTargetPlaceholder = $localize`:@@files.inbox.actionTargetPlaceholder:Folder ID or tag name`;

  // Icons
  readonly playIcon: SafeHtml;
  readonly plusIcon: SafeHtml;
  readonly pencilIcon: SafeHtml;
  readonly trashIcon: SafeHtml;
  readonly checkIcon: SafeHtml;
  readonly closeIcon: SafeHtml;
  readonly inboxIconLg: SafeHtml;
  readonly clockIconLg: SafeHtml;

  constructor() {
    this.playIcon = this.sanitizer.bypassSecurityTrustHtml(ICONS.PLAY);
    this.plusIcon = this.sanitizer.bypassSecurityTrustHtml(ICONS.PLUS);
    this.pencilIcon = this.sanitizer.bypassSecurityTrustHtml(ICONS.PENCIL);
    this.trashIcon = this.sanitizer.bypassSecurityTrustHtml(ICONS.TRASH);
    this.checkIcon = this.sanitizer.bypassSecurityTrustHtml(ICONS.CHECK);
    this.closeIcon = this.sanitizer.bypassSecurityTrustHtml(ICONS.CLOSE);
    const lgInbox = ICONS.INBOX.replace('h-5 w-5', 'h-12 w-12');
    this.inboxIconLg = this.sanitizer.bypassSecurityTrustHtml(lgInbox);
    const lgClock = ICONS.CLOCK.replace('h-5 w-5', 'h-12 w-12');
    this.clockIconLg = this.sanitizer.bypassSecurityTrustHtml(lgClock);
  }

  ngOnInit(): void {
    this.loadRules();
    this.loadLog();
  }

  toggleRule(rule: OrganizationRuleDto): void {
    this.inboxService.toggleRule(rule.id, !rule.isEnabled, this.familyId).subscribe((ok) => {
      if (ok) {
        this.rules.update((rules) =>
          rules.map((r) => (r.id === rule.id ? { ...r, isEnabled: !r.isEnabled } : r)),
        );
      }
    });
  }

  editRule(rule: OrganizationRuleDto): void {
    this.editingRule.set(rule);
    this.ruleName = rule.name;
    this.actionType = rule.actionType;
    try {
      const conditions = JSON.parse(rule.conditionsJson);
      if (Array.isArray(conditions) && conditions.length > 0) {
        this.conditionField = conditions[0].field ?? 'extension';
        this.conditionValue = conditions[0].value ?? '';
      }
    } catch {
      this.conditionField = 'extension';
      this.conditionValue = '';
    }
    try {
      const actions = JSON.parse(rule.actionsJson);
      this.actionTarget = actions.targetFolderId ?? actions.tagNames?.join(', ') ?? '';
    } catch {
      this.actionTarget = '';
    }
    this.showRuleEditor.set(true);
  }

  deleteRule(rule: OrganizationRuleDto): void {
    this.inboxService.deleteRule(rule.id, this.familyId).subscribe((ok) => {
      if (ok) {
        this.rules.update((rules) => rules.filter((r) => r.id !== rule.id));
      }
    });
  }

  processInbox(): void {
    this.processing.set(true);
    this.inboxService.processInboxFiles(this.familyId).subscribe((result) => {
      this.processing.set(false);
      if (result.success) {
        this.lastResult.set({
          filesProcessed: result.filesProcessed,
          rulesMatched: result.rulesMatched,
        });
        this.loadLog();
      }
    });
  }

  saveRule(): void {
    const conditionsJson = JSON.stringify([
      { field: this.conditionField, operator: 'matches', value: this.conditionValue },
    ]);
    const actionsJson = JSON.stringify({
      targetFolderId: this.actionType === 'MoveToFolder' ? this.actionTarget : undefined,
      tagNames:
        this.actionType === 'ApplyTag'
          ? this.actionTarget.split(',').map((t) => t.trim())
          : undefined,
    });

    if (this.editingRule()) {
      this.inboxService
        .updateRule({
          ruleId: this.editingRule()!.id,
          name: this.ruleName,
          familyId: this.familyId,
          conditionsJson,
          conditionLogic: 'All',
          actionType: this.actionType,
          actionsJson,
        })
        .subscribe((ok) => {
          if (ok) {
            this.cancelRuleEditor();
            this.loadRules();
          }
        });
    } else {
      this.inboxService
        .createRule({
          name: this.ruleName,
          familyId: this.familyId,
          conditionsJson,
          conditionLogic: 'All',
          actionType: this.actionType,
          actionsJson,
        })
        .subscribe((ruleId) => {
          if (ruleId) {
            this.cancelRuleEditor();
            this.loadRules();
          }
        });
    }
  }

  cancelRuleEditor(): void {
    this.showRuleEditor.set(false);
    this.editingRule.set(null);
    this.ruleName = '';
    this.conditionField = 'extension';
    this.conditionValue = '';
    this.actionType = 'MoveToFolder';
    this.actionTarget = '';
  }

  private loadRules(): void {
    this.inboxService.getRules(this.familyId).subscribe((rules) => {
      this.rules.set(rules);
      this.loading.set(false);
    });
  }

  private loadLog(): void {
    this.inboxService.getProcessingLog(this.familyId, 0, 50).subscribe((entries) => {
      this.logEntries.set(entries);
    });
  }
}
