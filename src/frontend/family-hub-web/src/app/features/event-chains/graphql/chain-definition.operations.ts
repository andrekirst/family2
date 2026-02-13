import { gql } from 'apollo-angular';

export const GET_CHAIN_DEFINITIONS = gql`
  query GetChainDefinitions($familyId: UUID!, $isEnabled: Boolean) {
    eventChain {
      chainDefinitions(familyId: $familyId, isEnabled: $isEnabled) {
        id
        familyId
        name
        description
        isEnabled
        isTemplate
        templateName
        trigger {
          eventType
          module
          description
          outputSchema
        }
        steps {
          alias
          name
          actionType
          actionVersion
          module
          inputMappings
          condition
          isCompensatable
          compensationActionType
          order
        }
        createdByUserId
        createdAt
        updatedAt
        version
        executionCount
        lastExecutedAt
      }
    }
  }
`;
