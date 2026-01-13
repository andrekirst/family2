/**
 * GraphQL Schema Introspection and Validation Helpers
 *
 * Provides utilities for schema introspection, normalization, and comparison.
 * Used for E2E tests to validate GraphQL schema stability.
 *
 * @see Issue #74 - E2E Tests - GraphQL Schema Validation
 */

import { APIRequestContext } from '@playwright/test';

// ============================================================================
// Type Definitions
// ============================================================================

/**
 * Recursive type reference structure from GraphQL introspection
 */
export interface TypeRef {
  kind: string;
  name: string | null;
  ofType: TypeRef | null;
}

/**
 * Field argument definition
 */
export interface InputValue {
  name: string;
  type: TypeRef;
  defaultValue: string | null;
}

/**
 * Field definition with arguments
 */
export interface Field {
  name: string;
  description: string | null;
  args: InputValue[];
  type: TypeRef;
  isDeprecated: boolean;
  deprecationReason: string | null;
}

/**
 * Enum value definition
 */
export interface EnumValue {
  name: string;
  isDeprecated: boolean;
  deprecationReason: string | null;
}

/**
 * Full type definition from introspection
 */
export interface IntrospectionType {
  kind: string;
  name: string;
  description: string | null;
  fields: Field[] | null;
  inputFields: InputValue[] | null;
  interfaces: TypeRef[] | null;
  enumValues: EnumValue[] | null;
  possibleTypes: TypeRef[] | null;
}

/**
 * Directive definition
 */
export interface Directive {
  name: string;
  description: string | null;
  locations: string[];
  args: InputValue[];
}

/**
 * Complete schema structure from introspection
 */
export interface IntrospectionSchema {
  queryType: { name: string } | null;
  mutationType: { name: string } | null;
  subscriptionType: { name: string } | null;
  types: IntrospectionType[];
  directives: Directive[];
}

/**
 * Introspection query response
 */
export interface IntrospectionResponse {
  data: {
    __schema: IntrospectionSchema;
  };
  errors?: { message: string }[];
}

/**
 * Normalized type for comparison
 */
export interface NormalizedType {
  kind: string;
  fields?: string[];
  inputFields?: string[];
  enumValues?: string[];
  possibleTypes?: string[];
  interfaces?: string[];
}

/**
 * Normalized schema for snapshot comparison
 */
export interface NormalizedSchema {
  queryFields: string[];
  mutationFields: string[];
  types: Record<string, NormalizedType>;
  errorTypes: string[];
  directives: string[];
}

/**
 * Breaking change detection result
 *
 * Currently implements detection for:
 * - REMOVED_TYPE: A type was removed from the schema
 * - REMOVED_FIELD: A field was removed from a type
 *
 * Future: NULLABILITY_CHANGE, TYPE_CHANGE (not yet implemented)
 */
export interface BreakingChange {
  type: 'REMOVED_TYPE' | 'REMOVED_FIELD';
  description: string;
  path: string;
}

// ============================================================================
// Constants
// ============================================================================

/**
 * Built-in GraphQL types to filter from schema comparisons
 */
const BUILT_IN_TYPES = new Set([
  // GraphQL introspection types
  '__Schema',
  '__Type',
  '__TypeKind',
  '__Field',
  '__InputValue',
  '__EnumValue',
  '__Directive',
  '__DirectiveLocation',
  // GraphQL scalar types
  'String',
  'Int',
  'Float',
  'Boolean',
  'ID',
  // Common scalar extensions
  'Date',
  'DateTime',
  'UUID',
  'Uuid',
  'Long',
  'Decimal',
  'TimeSpan',
  'Byte',
  'Short',
  'Any',
  'Upload',
  'Url',
  'Uri',
]);

/**
 * Hot Chocolate internal type patterns to filter
 */
const HOT_CHOCOLATE_TYPE_PATTERNS = [
  /^HotChocolate_/,
  /Connection$/,
  /Edge$/,
  /^PageInfo$/,
  /^CollectionSegmentInfo$/,
  /^ApplyFilterInput$/,
  /^ApplySortInput$/,
  /^FilterInput$/,
  /^SortInput$/,
  /^SortEnumType$/,
  /^SortOrder$/,
  /^ComparableOperationFilterInput/,
  /^StringOperationFilterInput$/,
  /^ListFilterInput/,
  /^BooleanOperationFilterInput$/,
  /^DateTimeOperationFilterInput$/,
  /^UuidOperationFilterInput$/,
];

/**
 * Standard GraphQL introspection query with full type depth
 * 7 levels of nesting handles complex types like [[String!]!]!
 */
export const INTROSPECTION_QUERY = `
  query IntrospectionQuery {
    __schema {
      queryType { name }
      mutationType { name }
      subscriptionType { name }
      types {
        kind
        name
        description
        fields(includeDeprecated: true) {
          name
          description
          args {
            name
            type { ...TypeRef }
            defaultValue
          }
          type { ...TypeRef }
          isDeprecated
          deprecationReason
        }
        inputFields {
          name
          type { ...TypeRef }
          defaultValue
        }
        interfaces { ...TypeRef }
        enumValues(includeDeprecated: true) {
          name
          isDeprecated
          deprecationReason
        }
        possibleTypes { ...TypeRef }
      }
      directives {
        name
        description
        locations
        args {
          name
          type { ...TypeRef }
          defaultValue
        }
      }
    }
  }

  fragment TypeRef on __Type {
    kind
    name
    ofType {
      kind
      name
      ofType {
        kind
        name
        ofType {
          kind
          name
          ofType {
            kind
            name
            ofType {
              kind
              name
              ofType {
                kind
                name
              }
            }
          }
        }
      }
    }
  }
`;

// ============================================================================
// Helper Functions
// ============================================================================

/**
 * Check if a type name should be filtered (built-in or Hot Chocolate internal)
 */
export function isFilteredType(typeName: string): boolean {
  if (BUILT_IN_TYPES.has(typeName)) {
    return true;
  }
  return HOT_CHOCOLATE_TYPE_PATTERNS.some((pattern) => pattern.test(typeName));
}

/**
 * Flatten a TypeRef to a readable string representation
 * e.g., { kind: 'NON_NULL', ofType: { kind: 'LIST', ofType: { kind: 'SCALAR', name: 'String' } } }
 * becomes '[String]!'
 */
export function flattenTypeRef(typeRef: TypeRef | null): string {
  if (!typeRef) return 'null';

  switch (typeRef.kind) {
    case 'NON_NULL':
      return `${flattenTypeRef(typeRef.ofType)}!`;
    case 'LIST':
      return `[${flattenTypeRef(typeRef.ofType)}]`;
    default:
      return typeRef.name || 'unknown';
  }
}

/**
 * Extract field names with their types as strings
 */
export function extractFieldSignatures(fields: Field[] | null): string[] {
  if (!fields) return [];
  return fields.map((f) => `${f.name}: ${flattenTypeRef(f.type)}`).sort();
}

/**
 * Extract just field names sorted alphabetically
 */
export function extractFieldNames(fields: Field[] | null): string[] {
  if (!fields) return [];
  return fields.map((f) => f.name).sort();
}

/**
 * Extract input field names sorted alphabetically
 */
export function extractInputFieldNames(inputFields: InputValue[] | null): string[] {
  if (!inputFields) return [];
  return inputFields.map((f) => f.name).sort();
}

/**
 * Extract enum value names sorted alphabetically
 */
export function extractEnumValues(enumValues: EnumValue[] | null): string[] {
  if (!enumValues) return [];
  return enumValues.map((e) => e.name).sort();
}

/**
 * Extract possible type names for unions sorted alphabetically
 */
export function extractPossibleTypes(possibleTypes: TypeRef[] | null): string[] {
  if (!possibleTypes) return [];
  return possibleTypes.map((t) => t.name || 'unknown').sort();
}

/**
 * Extract interface names sorted alphabetically
 */
export function extractInterfaces(interfaces: TypeRef[] | null): string[] {
  if (!interfaces) return [];
  return interfaces.map((i) => i.name || 'unknown').sort();
}

/**
 * Normalize a single type for snapshot comparison
 */
export function normalizeType(type: IntrospectionType): NormalizedType {
  const normalized: NormalizedType = {
    kind: type.kind,
  };

  if (type.fields && type.fields.length > 0) {
    normalized.fields = extractFieldNames(type.fields);
  }

  if (type.inputFields && type.inputFields.length > 0) {
    normalized.inputFields = extractInputFieldNames(type.inputFields);
  }

  if (type.enumValues && type.enumValues.length > 0) {
    normalized.enumValues = extractEnumValues(type.enumValues);
  }

  if (type.possibleTypes && type.possibleTypes.length > 0) {
    normalized.possibleTypes = extractPossibleTypes(type.possibleTypes);
  }

  if (type.interfaces && type.interfaces.length > 0) {
    normalized.interfaces = extractInterfaces(type.interfaces);
  }

  return normalized;
}

/**
 * Normalize entire schema for snapshot comparison
 * Filters built-in types, sorts alphabetically, extracts field names
 */
export function normalizeSchema(schema: IntrospectionSchema): NormalizedSchema {
  // Filter and sort types
  const domainTypes = schema.types
    .filter((t) => !isFilteredType(t.name))
    .sort((a, b) => a.name.localeCompare(b.name));

  // Build normalized types map
  const types: Record<string, NormalizedType> = {};
  for (const type of domainTypes) {
    types[type.name] = normalizeType(type);
  }

  // Extract Query fields
  const queryType = schema.types.find((t) => t.name === schema.queryType?.name);
  const queryFields = extractFieldNames(queryType?.fields || null);

  // Extract Mutation fields
  const mutationType = schema.types.find((t) => t.name === schema.mutationType?.name);
  const mutationFields = extractFieldNames(mutationType?.fields || null);

  // Extract error types (common pattern: types ending with 'Error')
  const errorTypes = domainTypes.filter((t) => t.name.endsWith('Error')).map((t) => t.name);

  // Extract directive names
  const directives = schema.directives.map((d) => d.name).sort();

  return {
    queryFields,
    mutationFields,
    types,
    errorTypes,
    directives,
  };
}

/**
 * Detect breaking changes between baseline and current schema
 */
export function detectBreakingChanges(
  baseline: NormalizedSchema,
  current: NormalizedSchema
): BreakingChange[] {
  const changes: BreakingChange[] = [];

  // Check for removed types
  for (const typeName of Object.keys(baseline.types)) {
    if (!current.types[typeName]) {
      changes.push({
        type: 'REMOVED_TYPE',
        description: `Type '${typeName}' was removed`,
        path: typeName,
      });
    }
  }

  // Check for removed fields in existing types
  for (const [typeName, baselineType] of Object.entries(baseline.types)) {
    const currentType = current.types[typeName];
    if (!currentType) continue;

    // Check fields
    if (baselineType.fields) {
      for (const fieldName of baselineType.fields) {
        if (!currentType.fields?.includes(fieldName)) {
          changes.push({
            type: 'REMOVED_FIELD',
            description: `Field '${fieldName}' was removed from type '${typeName}'`,
            path: `${typeName}.${fieldName}`,
          });
        }
      }
    }

    // Check input fields
    if (baselineType.inputFields) {
      for (const fieldName of baselineType.inputFields) {
        if (!currentType.inputFields?.includes(fieldName)) {
          changes.push({
            type: 'REMOVED_FIELD',
            description: `Input field '${fieldName}' was removed from type '${typeName}'`,
            path: `${typeName}.${fieldName}`,
          });
        }
      }
    }
  }

  // Check for removed query fields
  for (const queryField of baseline.queryFields) {
    if (!current.queryFields.includes(queryField)) {
      changes.push({
        type: 'REMOVED_FIELD',
        description: `Query field '${queryField}' was removed`,
        path: `Query.${queryField}`,
      });
    }
  }

  // Check for removed mutation fields
  for (const mutationField of baseline.mutationFields) {
    if (!current.mutationFields.includes(mutationField)) {
      changes.push({
        type: 'REMOVED_FIELD',
        description: `Mutation field '${mutationField}' was removed`,
        path: `Mutation.${mutationField}`,
      });
    }
  }

  return changes;
}

// ============================================================================
// Schema Client Class
// ============================================================================

/**
 * Client for GraphQL schema introspection and validation
 */
export class SchemaClient {
  private cachedSchema: IntrospectionSchema | null = null;

  constructor(
    private graphqlUrl: string,
    private request: APIRequestContext
  ) {}

  /**
   * Fetch the GraphQL schema via introspection query
   *
   * @param timeout - Optional timeout in milliseconds (default: 5000ms)
   */
  async fetchSchema(timeout = 5000): Promise<IntrospectionSchema> {
    if (this.cachedSchema) {
      return this.cachedSchema;
    }

    const response = await this.request.post(this.graphqlUrl, {
      data: {
        query: INTROSPECTION_QUERY,
      },
      timeout, // Add explicit timeout for introspection requests
    });

    if (!response.ok()) {
      throw new Error(
        `Schema introspection failed: ${response.statusText()}. ` +
          `Ensure the GraphQL endpoint is running at ${this.graphqlUrl}`
      );
    }

    const result: IntrospectionResponse = await response.json();

    if (result.errors && result.errors.length > 0) {
      throw new Error(`GraphQL errors: ${JSON.stringify(result.errors, null, 2)}`);
    }

    this.cachedSchema = result.data.__schema;
    return this.cachedSchema;
  }

  /**
   * Get normalized schema for snapshot comparison
   */
  async getNormalizedSchema(): Promise<NormalizedSchema> {
    const schema = await this.fetchSchema();
    return normalizeSchema(schema);
  }

  /**
   * Get list of all domain type names (filtered)
   */
  async getTypeNames(): Promise<string[]> {
    const schema = await this.fetchSchema();
    return schema.types
      .filter((t) => !isFilteredType(t.name))
      .map((t) => t.name)
      .sort();
  }

  /**
   * Get Query type field names
   */
  async getQueryFields(): Promise<string[]> {
    const schema = await this.fetchSchema();
    const queryType = schema.types.find((t) => t.name === schema.queryType?.name);
    return extractFieldNames(queryType?.fields || null);
  }

  /**
   * Get Mutation type field names
   */
  async getMutationFields(): Promise<string[]> {
    const schema = await this.fetchSchema();
    const mutationType = schema.types.find((t) => t.name === schema.mutationType?.name);
    return extractFieldNames(mutationType?.fields || null);
  }

  /**
   * Get a specific type by name
   */
  async getType(typeName: string): Promise<IntrospectionType | undefined> {
    const schema = await this.fetchSchema();
    return schema.types.find((t) => t.name === typeName);
  }

  /**
   * Get field signatures for a specific type
   */
  async getTypeFieldSignatures(typeName: string): Promise<string[]> {
    const type = await this.getType(typeName);
    return extractFieldSignatures(type?.fields || null);
  }

  /**
   * Check if a type exists in the schema
   */
  async hasType(typeName: string): Promise<boolean> {
    const typeNames = await this.getTypeNames();
    return typeNames.includes(typeName);
  }

  /**
   * Get all error types (types ending with 'Error')
   */
  async getErrorTypes(): Promise<string[]> {
    const typeNames = await this.getTypeNames();
    return typeNames.filter((name) => name.endsWith('Error'));
  }

  /**
   * Get union type members (possible types)
   */
  async getUnionMembers(unionTypeName: string): Promise<string[]> {
    const type = await this.getType(unionTypeName);
    return extractPossibleTypes(type?.possibleTypes || null);
  }

  /**
   * Get input type fields
   */
  async getInputTypeFields(inputTypeName: string): Promise<string[]> {
    const type = await this.getType(inputTypeName);
    return extractInputFieldNames(type?.inputFields || null);
  }

  /**
   * Clear cached schema (useful for tests that modify schema)
   */
  clearCache(): void {
    this.cachedSchema = null;
  }

  /**
   * Get raw schema (for advanced use cases)
   */
  async getRawSchema(): Promise<IntrospectionSchema> {
    return this.fetchSchema();
  }

  /**
   * Get schema size in KB (for performance monitoring)
   */
  async getSchemaSizeKB(): Promise<number> {
    const schema = await this.fetchSchema();
    const jsonString = JSON.stringify(schema);
    return jsonString.length / 1024;
  }
}
