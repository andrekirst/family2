# Event Chain Engine — GraphQL API Schema

Complete approved GraphQL API for the Event Chain Engine. Follows the Input→Command pattern (ADR-003) with Hot Chocolate.

---

## Types

### Core Types

```graphql
type ChainDefinition {
  id: UUID!
  familyId: UUID!
  name: String!
  description: String
  isEnabled: Boolean!
  isTemplate: Boolean!
  templateName: String
  trigger: TriggerDefinition!
  steps: [StepDefinition!]!
  createdByUserId: UUID!
  createdAt: DateTime!
  updatedAt: DateTime!
  version: Int!
  executionCount: Int!
  lastExecutedAt: DateTime
}

type TriggerDefinition {
  eventType: String!
  module: String!
  description: String!
  outputSchema: JSON!
}

type StepDefinition {
  alias: String!
  name: String!
  actionType: String!
  actionVersion: String!
  module: String!
  inputMappings: JSON!
  condition: String
  isCompensatable: Boolean!
  compensationActionType: String
  order: Int!
}

type ChainExecution {
  id: UUID!
  chainDefinitionId: UUID!
  chainDefinition: ChainDefinition!
  familyId: UUID!
  correlationId: UUID!
  status: ChainExecutionStatus!
  triggerEventType: String!
  triggerPayload: JSON!
  context: JSON!
  startedAt: DateTime!
  completedAt: DateTime
  failedAt: DateTime
  errorMessage: String
  stepExecutions: [StepExecution!]!
}

type StepExecution {
  id: UUID!
  chainExecutionId: UUID!
  stepAlias: String!
  stepName: String!
  actionType: String!
  status: StepExecutionStatus!
  inputPayload: JSON!
  outputPayload: JSON
  errorMessage: String
  retryCount: Int!
  startedAt: DateTime
  completedAt: DateTime
  compensatedAt: DateTime
}

type ChainEntityMapping {
  id: UUID!
  chainExecutionId: UUID!
  stepAlias: String!
  entityType: String!
  entityId: UUID!
  module: String!
  createdAt: DateTime!
}
```

### Enums

```graphql
enum ChainExecutionStatus {
  PENDING
  RUNNING
  COMPLETED
  PARTIALLY_COMPLETED
  FAILED
  COMPENSATING
  COMPENSATED
}

enum StepExecutionStatus {
  PENDING
  RUNNING
  COMPLETED
  FAILED
  SKIPPED
  COMPENSATING
  COMPENSATED
}
```

### Catalog Types (from Plugin Registry)

```graphql
type ActionCatalogEntry {
  actionType: String!
  module: String!
  name: String!
  description: String!
  version: String!
  inputSchema: JSON!
  outputSchema: JSON!
  isCompensatable: Boolean!
  isDeprecated: Boolean!
}

type TriggerCatalogEntry {
  eventType: String!
  module: String!
  name: String!
  description: String!
  outputSchema: JSON!
}

type ChainTemplate {
  name: String!
  description: String!
  category: String!
  trigger: TriggerCatalogEntry!
  steps: [StepDefinition!]!
  estimatedTimeSaved: String!
  mentalLoadReduction: String!
}
```

### Paginated Types

```graphql
type ChainDefinitionConnection {
  nodes: [ChainDefinition!]!
  totalCount: Int!
  pageInfo: PageInfo!
}

type ChainExecutionConnection {
  nodes: [ChainExecution!]!
  totalCount: Int!
  pageInfo: PageInfo!
}

type PageInfo {
  hasNextPage: Boolean!
  hasPreviousPage: Boolean!
  startCursor: String
  endCursor: String
}
```

---

## Queries

```graphql
type Query {
  # Chain Definitions
  chainDefinitions(
    familyId: UUID!
    isEnabled: Boolean
    first: Int
    after: String
  ): ChainDefinitionConnection!

  chainDefinition(id: UUID!): ChainDefinition

  # Chain Executions
  chainExecutions(
    familyId: UUID!
    chainDefinitionId: UUID
    status: ChainExecutionStatus
    first: Int
    after: String
  ): ChainExecutionConnection!

  chainExecution(id: UUID!): ChainExecution

  # Catalog (trigger/action registry)
  availableTriggers: [TriggerCatalogEntry!]!

  availableActions(
    compatibleWithTrigger: String
  ): [ActionCatalogEntry!]!

  # Templates
  chainTemplates: [ChainTemplate!]!

  # Entity Tracing
  chainEntityMappings(
    entityId: UUID!
    entityType: String
  ): [ChainEntityMapping!]!
}
```

---

## Mutations

### Input Types

```graphql
input CreateChainDefinitionInput {
  name: String!
  description: String
  triggerEventType: String!
  steps: [CreateStepInput!]!
  isEnabled: Boolean! = true
}

input CreateStepInput {
  alias: String!
  name: String!
  actionType: String!
  actionVersion: String!
  inputMappings: JSON!
  condition: String
  order: Int!
}

input UpdateChainDefinitionInput {
  name: String
  description: String
  steps: [UpdateStepInput!]
  isEnabled: Boolean
}

input UpdateStepInput {
  alias: String!
  name: String
  actionType: String
  actionVersion: String
  inputMappings: JSON
  condition: String
  order: Int
}

input InstallTemplateInput {
  templateName: String!
  customName: String
  isEnabled: Boolean! = true
}
```

### Mutation Operations

```graphql
type Mutation {
  # Chain Definition CRUD
  createChainDefinition(
    input: CreateChainDefinitionInput!
  ): CreateChainDefinitionPayload!

  updateChainDefinition(
    id: UUID!
    input: UpdateChainDefinitionInput!
  ): UpdateChainDefinitionPayload!

  deleteChainDefinition(
    id: UUID!
  ): DeleteChainDefinitionPayload!

  enableChainDefinition(
    id: UUID!
  ): ChainDefinition!

  disableChainDefinition(
    id: UUID!
  ): ChainDefinition!

  # Template Installation
  installChainTemplate(
    input: InstallTemplateInput!
  ): ChainDefinition!

  # Manual Execution (for testing)
  executeChainManually(
    chainDefinitionId: UUID!
    triggerPayload: JSON!
  ): ChainExecution!

  # Execution Management
  retryChainExecution(
    id: UUID!
  ): ChainExecution!

  cancelChainExecution(
    id: UUID!
  ): ChainExecution!
}
```

### Payload Types

```graphql
type CreateChainDefinitionPayload {
  chainDefinition: ChainDefinition
  errors: [UserError!]
}

type UpdateChainDefinitionPayload {
  chainDefinition: ChainDefinition
  errors: [UserError!]
}

type DeleteChainDefinitionPayload {
  success: Boolean!
  errors: [UserError!]
}

type UserError {
  message: String!
  code: String!
  field: String
}
```

---

## Subscriptions

```graphql
type Subscription {
  # Real-time execution updates
  chainExecutionUpdated(
    chainDefinitionId: UUID
    familyId: UUID!
  ): ChainExecution!

  # Step-level updates
  stepExecutionUpdated(
    chainExecutionId: UUID!
  ): StepExecution!
}
```

---

## Backend Mapping (C# Types)

### GraphQL Input → Command Mapping

| GraphQL Input | C# Command | Key Vogen Types |
|---------------|------------|-----------------|
| `CreateChainDefinitionInput` | `CreateChainDefinitionCommand` | `ChainDefinitionId`, `ChainName`, `StepAlias`, `ActionVersion` |
| `UpdateChainDefinitionInput` | `UpdateChainDefinitionCommand` | `ChainDefinitionId`, `ChainName` |
| `InstallTemplateInput` | `InstallChainTemplateCommand` | `ChainDefinitionId`, `ChainName` |
| `executeChainManually` | `ExecuteChainCommand` | `ChainDefinitionId`, `ChainExecutionId` |

### GraphQL Type → C# DTO Mapping

| GraphQL Type | C# DTO |
|-------------|--------|
| `ChainDefinition` | `ChainDefinitionDto` |
| `ChainExecution` | `ChainExecutionDto` |
| `StepExecution` | `StepExecutionDto` |
| `TriggerDefinition` | `TriggerDefinitionDto` |
| `StepDefinition` | `StepDefinitionDto` |
| `ActionCatalogEntry` | `ActionCatalogEntryDto` |
| `TriggerCatalogEntry` | `TriggerCatalogEntryDto` |
| `ChainTemplate` | `ChainTemplateDto` |
| `ChainEntityMapping` | `ChainEntityMappingDto` |

---

## Validation Rules

### CreateChainDefinitionInput

- `name`: Required, 1-200 characters
- `triggerEventType`: Must exist in trigger catalog
- `steps`: At least 1 step required
- `steps[].alias`: Unique within chain, 1-50 characters, alphanumeric + underscore
- `steps[].actionType`: Must exist in action catalog
- `steps[].actionVersion`: Must be a valid non-deprecated version
- `steps[].inputMappings`: Must be type-compatible with previous step outputs
- `steps[].order`: Sequential, starting from 1

### UpdateChainDefinitionInput

- Same field-level rules as create
- Cannot update while chain is executing
- Version check for optimistic concurrency

### Type Compatibility

Action inputs are validated against available context:

1. Trigger output schema defines initial context
2. Each step's output schema extends the context
3. Subsequent step input mappings validated against accumulated context
4. Validation uses JSON Schema compatibility checks

---

## Authorization

All operations require `[Authorize]`. Family-scoped access:

- **Queries**: RLS automatically filters by `app.current_family_id`
- **Mutations**: Verify user belongs to the family owning the chain
- **Subscriptions**: Scoped to family via `familyId` parameter + RLS

Role-based visibility:

- **Parents/Admins**: Full CRUD on all family chains
- **Members**: View all, create/edit own chains only
- **Children**: View simplified execution status only
