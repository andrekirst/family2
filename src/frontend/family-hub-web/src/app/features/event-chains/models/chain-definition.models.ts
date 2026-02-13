export interface TriggerDefinitionDto {
  eventType: string;
  module: string;
  description: string;
  outputSchema: string;
}

export interface StepDefinitionDto {
  alias: string;
  name: string;
  actionType: string;
  actionVersion: string;
  module: string;
  inputMappings: string;
  condition: string | null;
  isCompensatable: boolean;
  compensationActionType: string | null;
  order: number;
}

export interface ChainDefinitionDto {
  id: string;
  familyId: string;
  name: string;
  description: string | null;
  isEnabled: boolean;
  isTemplate: boolean;
  templateName: string | null;
  trigger: TriggerDefinitionDto;
  steps: StepDefinitionDto[];
  createdByUserId: string;
  createdAt: string;
  updatedAt: string;
  version: number;
  executionCount: number;
  lastExecutedAt: string | null;
}
