import { gql } from 'apollo-angular';

export const GET_ORGANIZATION_RULES = gql`
  query GetOrganizationRules {
    fileManagement {
      organizationRules {
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
  query GetProcessingLog($skip: Int!, $take: Int!) {
    fileManagement {
      processingLog(skip: $skip, take: $take) {
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
  query PreviewRuleMatch($fileId: UUID!) {
    fileManagement {
      previewRuleMatch(fileId: $fileId) {
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
    $conditionsJson: String!
    $conditionLogic: String!
    $actionType: String!
    $actionsJson: String!
  ) {
    fileManagement {
      createOrganizationRule(
        name: $name
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
    $conditionsJson: String!
    $conditionLogic: String!
    $actionType: String!
    $actionsJson: String!
  ) {
    fileManagement {
      updateOrganizationRule(
        ruleId: $ruleId
        name: $name
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
  mutation DeleteOrganizationRule($ruleId: UUID!) {
    fileManagement {
      deleteOrganizationRule(ruleId: $ruleId) {
        success
      }
    }
  }
`;

export const TOGGLE_ORGANIZATION_RULE = gql`
  mutation ToggleOrganizationRule($ruleId: UUID!, $isEnabled: Boolean!) {
    fileManagement {
      toggleOrganizationRule(ruleId: $ruleId, isEnabled: $isEnabled) {
        success
      }
    }
  }
`;

export const REORDER_ORGANIZATION_RULES = gql`
  mutation ReorderOrganizationRules($ruleIdsInOrder: [UUID!]!) {
    fileManagement {
      reorderOrganizationRules(ruleIdsInOrder: $ruleIdsInOrder) {
        success
      }
    }
  }
`;

export const PROCESS_INBOX_FILES = gql`
  mutation ProcessInboxFiles {
    fileManagement {
      processInboxFiles {
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
