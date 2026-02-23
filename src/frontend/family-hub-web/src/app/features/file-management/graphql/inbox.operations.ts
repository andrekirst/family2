import { gql } from 'apollo-angular';

export const GET_ORGANIZATION_RULES = gql`
  query GetOrganizationRules($familyId: UUID!) {
    fileManagement {
      getOrganizationRules(familyId: $familyId) {
        id
        name
        conditionsJson
        conditionLogic
        actionType
        actionsJson
        priority
        isEnabled
        createdBy
        createdAt
        updatedAt
      }
    }
  }
`;

export const GET_PROCESSING_LOG = gql`
  query GetProcessingLog($familyId: UUID!, $skip: Int!, $take: Int!) {
    fileManagement {
      getProcessingLog(familyId: $familyId, skip: $skip, take: $take) {
        id
        fileId
        fileName
        matchedRuleId
        matchedRuleName
        actionTaken
        destinationFolderId
        appliedTagNames
        success
        errorMessage
        processedAt
      }
    }
  }
`;

export const PREVIEW_RULE_MATCH = gql`
  query PreviewRuleMatch($fileId: UUID!, $familyId: UUID!) {
    fileManagement {
      previewRuleMatch(fileId: $fileId, familyId: $familyId) {
        matched
        matchedRuleId
        matchedRuleName
        actionType
        actionsJson
      }
    }
  }
`;

export const CREATE_ORGANIZATION_RULE = gql`
  mutation CreateOrganizationRule(
    $name: String!
    $familyId: UUID!
    $conditionsJson: String!
    $conditionLogic: String!
    $actionType: String!
    $actionsJson: String!
  ) {
    fileManagement {
      createOrganizationRule(
        name: $name
        familyId: $familyId
        conditionsJson: $conditionsJson
        conditionLogic: $conditionLogic
        actionType: $actionType
        actionsJson: $actionsJson
      ) {
        success
        ruleId
      }
    }
  }
`;

export const UPDATE_ORGANIZATION_RULE = gql`
  mutation UpdateOrganizationRule(
    $ruleId: UUID!
    $name: String!
    $familyId: UUID!
    $conditionsJson: String!
    $conditionLogic: String!
    $actionType: String!
    $actionsJson: String!
  ) {
    fileManagement {
      updateOrganizationRule(
        ruleId: $ruleId
        name: $name
        familyId: $familyId
        conditionsJson: $conditionsJson
        conditionLogic: $conditionLogic
        actionType: $actionType
        actionsJson: $actionsJson
      ) {
        success
      }
    }
  }
`;

export const DELETE_ORGANIZATION_RULE = gql`
  mutation DeleteOrganizationRule($ruleId: UUID!, $familyId: UUID!) {
    fileManagement {
      deleteOrganizationRule(ruleId: $ruleId, familyId: $familyId) {
        success
      }
    }
  }
`;

export const TOGGLE_ORGANIZATION_RULE = gql`
  mutation ToggleOrganizationRule($ruleId: UUID!, $isEnabled: Boolean!, $familyId: UUID!) {
    fileManagement {
      toggleOrganizationRule(ruleId: $ruleId, isEnabled: $isEnabled, familyId: $familyId) {
        success
      }
    }
  }
`;

export const REORDER_ORGANIZATION_RULES = gql`
  mutation ReorderOrganizationRules($ruleIdsInOrder: [UUID!]!, $familyId: UUID!) {
    fileManagement {
      reorderOrganizationRules(ruleIdsInOrder: $ruleIdsInOrder, familyId: $familyId) {
        success
      }
    }
  }
`;

export const PROCESS_INBOX_FILES = gql`
  mutation ProcessInboxFiles($familyId: UUID!) {
    fileManagement {
      processInboxFiles(familyId: $familyId) {
        success
        filesProcessed
        rulesMatched
        logEntries {
          id
          fileId
          fileName
          matchedRuleId
          matchedRuleName
          actionTaken
          destinationFolderId
          appliedTagNames
          success
          errorMessage
          processedAt
        }
      }
    }
  }
`;
