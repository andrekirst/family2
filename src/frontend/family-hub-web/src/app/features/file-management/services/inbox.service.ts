import { Injectable, inject } from '@angular/core';
import { Apollo } from 'apollo-angular';
import { Observable, catchError, map, of } from 'rxjs';
import {
  OrganizationRuleDto,
  ProcessingLogEntryDto,
  RuleMatchPreviewDto,
  CreateOrganizationRuleInput,
  UpdateOrganizationRuleInput,
} from '../models/inbox.models';
import {
  GET_ORGANIZATION_RULES,
  GET_PROCESSING_LOG,
  PREVIEW_RULE_MATCH,
  CREATE_ORGANIZATION_RULE,
  UPDATE_ORGANIZATION_RULE,
  DELETE_ORGANIZATION_RULE,
  TOGGLE_ORGANIZATION_RULE,
  REORDER_ORGANIZATION_RULES,
  PROCESS_INBOX_FILES,
} from '../graphql/inbox.operations';

@Injectable({ providedIn: 'root' })
export class InboxService {
  private readonly apollo = inject(Apollo);

  getRules(): Observable<OrganizationRuleDto[]> {
    return this.apollo
      .query<{ fileManagement: { organizationRules: OrganizationRuleDto[] } }>({
        query: GET_ORGANIZATION_RULES,
        fetchPolicy: 'network-only',
      })
      .pipe(
        map((r) => r.data?.fileManagement?.organizationRules ?? []),
        catchError((err) => {
          console.error('Failed to load organization rules:', err);
          return of([]);
        }),
      );
  }

  getProcessingLog(skip = 0, take = 20): Observable<ProcessingLogEntryDto[]> {
    return this.apollo
      .query<{ fileManagement: { processingLog: ProcessingLogEntryDto[] } }>({
        query: GET_PROCESSING_LOG,
        variables: { skip, take },
        fetchPolicy: 'network-only',
      })
      .pipe(
        map((r) => r.data?.fileManagement?.processingLog ?? []),
        catchError((err) => {
          console.error('Failed to load processing log:', err);
          return of([]);
        }),
      );
  }

  previewRuleMatch(fileId: string): Observable<RuleMatchPreviewDto | null> {
    return this.apollo
      .query<{ fileManagement: { previewRuleMatch: RuleMatchPreviewDto | null } }>({
        query: PREVIEW_RULE_MATCH,
        variables: { fileId },
        fetchPolicy: 'network-only',
      })
      .pipe(
        map((r) => r.data?.fileManagement?.previewRuleMatch ?? null),
        catchError((err) => {
          console.error('Failed to preview rule match:', err);
          return of(null);
        }),
      );
  }

  createRule(input: CreateOrganizationRuleInput): Observable<string | null> {
    return this.apollo
      .mutate<{ fileManagement: { createOrganizationRule: { success: boolean; ruleId: string } } }>(
        {
          mutation: CREATE_ORGANIZATION_RULE,
          variables: input,
        },
      )
      .pipe(
        map((r) => r.data?.fileManagement.createOrganizationRule.ruleId ?? null),
        catchError((err) => {
          console.error('Failed to create organization rule:', err);
          return of(null);
        }),
      );
  }

  updateRule(input: UpdateOrganizationRuleInput): Observable<boolean> {
    return this.apollo
      .mutate<{ fileManagement: { updateOrganizationRule: { success: boolean } } }>({
        mutation: UPDATE_ORGANIZATION_RULE,
        variables: input,
      })
      .pipe(
        map((r) => r.data?.fileManagement.updateOrganizationRule.success ?? false),
        catchError((err) => {
          console.error('Failed to update organization rule:', err);
          return of(false);
        }),
      );
  }

  deleteRule(ruleId: string): Observable<boolean> {
    return this.apollo
      .mutate<{ fileManagement: { deleteOrganizationRule: { success: boolean } } }>({
        mutation: DELETE_ORGANIZATION_RULE,
        variables: { ruleId },
      })
      .pipe(
        map((r) => r.data?.fileManagement.deleteOrganizationRule.success ?? false),
        catchError((err) => {
          console.error('Failed to delete organization rule:', err);
          return of(false);
        }),
      );
  }

  toggleRule(ruleId: string, isEnabled: boolean): Observable<boolean> {
    return this.apollo
      .mutate<{ fileManagement: { toggleOrganizationRule: { success: boolean } } }>({
        mutation: TOGGLE_ORGANIZATION_RULE,
        variables: { ruleId, isEnabled },
      })
      .pipe(
        map((r) => r.data?.fileManagement.toggleOrganizationRule.success ?? false),
        catchError((err) => {
          console.error('Failed to toggle organization rule:', err);
          return of(false);
        }),
      );
  }

  reorderRules(ruleIdsInOrder: string[]): Observable<boolean> {
    return this.apollo
      .mutate<{ fileManagement: { reorderOrganizationRules: { success: boolean } } }>({
        mutation: REORDER_ORGANIZATION_RULES,
        variables: { ruleIdsInOrder },
      })
      .pipe(
        map((r) => r.data?.fileManagement.reorderOrganizationRules.success ?? false),
        catchError((err) => {
          console.error('Failed to reorder organization rules:', err);
          return of(false);
        }),
      );
  }

  processInboxFiles(): Observable<{
    success: boolean;
    filesProcessed: number;
    rulesMatched: number;
    logEntries: ProcessingLogEntryDto[];
  }> {
    return this.apollo
      .mutate<{
        fileManagement: {
          processInboxFiles: {
            success: boolean;
            filesProcessed: number;
            rulesMatched: number;
            logEntries: ProcessingLogEntryDto[];
          };
        };
      }>({
        mutation: PROCESS_INBOX_FILES,
      })
      .pipe(
        map(
          (r) =>
            r.data?.fileManagement.processInboxFiles ?? {
              success: false,
              filesProcessed: 0,
              rulesMatched: 0,
              logEntries: [],
            },
        ),
        catchError((err) => {
          console.error('Failed to process inbox files:', err);
          return of({ success: false, filesProcessed: 0, rulesMatched: 0, logEntries: [] });
        }),
      );
  }
}
