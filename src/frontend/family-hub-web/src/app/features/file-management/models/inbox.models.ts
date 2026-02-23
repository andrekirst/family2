export interface OrganizationRuleDto {
  id: string;
  name: string;
  conditionsJson: string;
  conditionLogic: string;
  actionType: string;
  actionsJson: string;
  priority: number;
  isEnabled: boolean;
  createdBy: string;
  createdAt: string;
  updatedAt: string;
}

export interface ProcessingLogEntryDto {
  id: string;
  fileId: string;
  fileName: string;
  matchedRuleId: string | null;
  matchedRuleName: string | null;
  actionTaken: string | null;
  destinationFolderId: string | null;
  appliedTagNames: string | null;
  success: boolean;
  errorMessage: string | null;
  processedAt: string;
}

export interface RuleMatchPreviewDto {
  matched: boolean;
  matchedRuleId: string | null;
  matchedRuleName: string | null;
  actionType: string | null;
  actionsJson: string | null;
}

export interface CreateOrganizationRuleInput {
  name: string;
  familyId: string;
  conditionsJson: string;
  conditionLogic: string;
  actionType: string;
  actionsJson: string;
}

export interface UpdateOrganizationRuleInput {
  ruleId: string;
  name: string;
  familyId: string;
  conditionsJson: string;
  conditionLogic: string;
  actionType: string;
  actionsJson: string;
}

/** Parsed condition structure for UI rendering */
export interface RuleCondition {
  field: string;
  operator: string;
  value: string;
}

/** Parsed action structure for UI rendering */
export interface RuleAction {
  targetFolderId?: string;
  targetFolderName?: string;
  tagNames?: string[];
}
