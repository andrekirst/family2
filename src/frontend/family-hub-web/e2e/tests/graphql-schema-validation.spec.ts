/**
 * GraphQL Schema Validation E2E Tests
 *
 * Tests for validating GraphQL schema stability, structure, and backwards compatibility.
 * Uses Playwright's native snapshot testing for schema comparison.
 *
 * Test Categories:
 * 1. Schema Introspection - Verify introspection works and returns valid schema
 * 2. Schema Snapshots - Compare schema against baseline snapshots
 * 3. Module Type Validation - Verify expected domain types exist
 * 4. Critical Query Structure - Validate important queries/mutations
 * 5. Breaking Change Detection - Detect removed types/fields
 * 6. Performance - Monitor schema size and introspection time
 *
 * @see Issue #74 - E2E Tests - GraphQL Schema Validation
 */

import { test, expect } from '../fixtures/schema.fixture';
import { SCHEMA, TIMEOUTS } from '../support/constants';
import { detectBreakingChanges, NormalizedSchema } from '../support/schema-helpers';

// ============================================================================
// Test Suite 1: Schema Introspection Tests
// ============================================================================

test.describe('GraphQL Schema - Introspection', () => {
  test('should successfully introspect schema', async ({ schemaClient }) => {
    await test.step('Execute introspection query', async () => {
      const schema = await schemaClient.getRawSchema();
      expect(schema).toBeDefined();
      expect(schema.types).toBeDefined();
      expect(schema.types.length).toBeGreaterThan(0);
    });
  });

  test('should have Query root type', async ({ schemaClient }) => {
    await test.step('Verify Query type exists', async () => {
      const schema = await schemaClient.getRawSchema();
      expect(schema.queryType).toBeDefined();
      expect(schema.queryType?.name).toBe('Query');
    });

    await test.step('Verify Query has fields', async () => {
      const queryFields = await schemaClient.getQueryFields();
      expect(queryFields.length).toBeGreaterThan(0);
    });
  });

  test('should have Mutation root type', async ({ schemaClient }) => {
    await test.step('Verify Mutation type exists', async () => {
      const schema = await schemaClient.getRawSchema();
      expect(schema.mutationType).toBeDefined();
      expect(schema.mutationType?.name).toBe('Mutation');
    });

    await test.step('Verify Mutation has fields', async () => {
      const mutationFields = await schemaClient.getMutationFields();
      expect(mutationFields.length).toBeGreaterThan(0);
    });
  });

  test('should have Subscription root type', async ({ schemaClient }) => {
    await test.step('Verify Subscription type exists', async () => {
      const schema = await schemaClient.getRawSchema();
      // Subscriptions implemented in issue #84 (GraphQL Real-Time Subscriptions)
      expect(schema.subscriptionType).not.toBeNull();
      expect(schema.subscriptionType?.name).toBe('Subscription');
    });
  });
});

// ============================================================================
// Test Suite 2: Schema Snapshot Tests
// ============================================================================

test.describe('GraphQL Schema - Snapshots', () => {
  test('should match full schema snapshot', async ({ schemaClient }) => {
    await test.step('Get normalized schema', async () => {
      const normalizedSchema = await schemaClient.getNormalizedSchema();

      // Use Playwright's native snapshot testing
      // Schema is normalized (sorted, filtered) for deterministic comparison
      expect(JSON.stringify(normalizedSchema, null, 2)).toMatchSnapshot('graphql-schema-full.json');
    });
  });

  test('should match Query type fields snapshot', async ({ schemaClient }) => {
    await test.step('Get Query fields', async () => {
      const queryFields = await schemaClient.getQueryFields();
      expect(JSON.stringify(queryFields, null, 2)).toMatchSnapshot(
        'graphql-schema-query-fields.json'
      );
    });
  });

  test('should match Mutation type fields snapshot', async ({ schemaClient }) => {
    await test.step('Get Mutation fields', async () => {
      const mutationFields = await schemaClient.getMutationFields();
      expect(JSON.stringify(mutationFields, null, 2)).toMatchSnapshot(
        'graphql-schema-mutation-fields.json'
      );
    });
  });

  test('should match error types snapshot', async ({ schemaClient }) => {
    await test.step('Get error types', async () => {
      const errorTypes = await schemaClient.getErrorTypes();
      expect(JSON.stringify(errorTypes, null, 2)).toMatchSnapshot(
        'graphql-schema-error-types.json'
      );
    });
  });
});

// ============================================================================
// Test Suite 3: Module Type Validation
// ============================================================================

test.describe('GraphQL Schema - Module Types', () => {
  test('should include Auth module types', async ({ schemaClient }) => {
    const typeNames = await schemaClient.getTypeNames();

    await test.step('Verify Auth module types exist', async () => {
      for (const expectedType of SCHEMA.AUTH_MODULE_TYPES) {
        expect(typeNames, `Missing Auth type: ${expectedType}`).toContain(expectedType);
      }
    });
  });

  test('should include Family module types', async ({ schemaClient }) => {
    const typeNames = await schemaClient.getTypeNames();

    await test.step('Verify Family module types exist', async () => {
      for (const expectedType of SCHEMA.FAMILY_MODULE_TYPES) {
        expect(typeNames, `Missing Family type: ${expectedType}`).toContain(expectedType);
      }
    });
  });

  test('should include error types (mutation conventions)', async ({ schemaClient }) => {
    const errorTypes = await schemaClient.getErrorTypes();

    await test.step('Verify error types exist', async () => {
      for (const expectedError of SCHEMA.ERROR_TYPES) {
        expect(errorTypes, `Missing error type: ${expectedError}`).toContain(expectedError);
      }
    });
  });
});

// ============================================================================
// Test Suite 4: Critical Query Structure Tests
// ============================================================================

test.describe('GraphQL Schema - Critical Queries', () => {
  test('should have critical Query fields', async ({ schemaClient }) => {
    const queryFields = await schemaClient.getQueryFields();

    await test.step('Verify critical query fields exist', async () => {
      for (const criticalField of SCHEMA.CRITICAL_QUERY_FIELDS) {
        expect(queryFields, `Missing critical query: ${criticalField}`).toContain(criticalField);
      }
    });
  });

  test('should have critical Mutation fields', async ({ schemaClient }) => {
    const mutationFields = await schemaClient.getMutationFields();

    await test.step('Verify critical mutation fields exist', async () => {
      for (const criticalField of SCHEMA.CRITICAL_MUTATION_FIELDS) {
        expect(mutationFields, `Missing critical mutation: ${criticalField}`).toContain(
          criticalField
        );
      }
    });
  });

  test('CreateFamily mutation should have correct input type', async ({ schemaClient }) => {
    await test.step('Verify CreateFamilyInput exists', async () => {
      const hasType = await schemaClient.hasType('CreateFamilyInput');
      expect(hasType).toBe(true);
    });

    await test.step('Verify CreateFamilyInput has name field', async () => {
      const inputFields = await schemaClient.getInputTypeFields('CreateFamilyInput');
      expect(inputFields).toContain('name');
    });
  });

  test('CreateFamily mutation should have correct payload type', async ({ schemaClient }) => {
    await test.step('Verify CreateFamilyPayload exists', async () => {
      const hasType = await schemaClient.hasType('CreateFamilyPayload');
      expect(hasType).toBe(true);
    });

    await test.step('Verify payload has expected fields', async () => {
      const type = await schemaClient.getType('CreateFamilyPayload');
      const fieldNames = type?.fields?.map((f) => f.name) || [];

      // Hot Chocolate MutationConventions pattern
      expect(fieldNames).toContain('errors');
    });
  });

  test('Family type should have required fields', async ({ schemaClient }) => {
    await test.step('Verify Family type exists', async () => {
      const hasType = await schemaClient.hasType('Family');
      expect(hasType).toBe(true);
    });

    await test.step('Verify Family has id and name fields', async () => {
      const type = await schemaClient.getType('Family');
      const fieldNames = type?.fields?.map((f) => f.name) || [];

      expect(fieldNames).toContain('id');
      expect(fieldNames).toContain('name');
    });
  });

  test('User type should have required fields', async ({ schemaClient }) => {
    await test.step('Verify User type exists', async () => {
      const hasType = await schemaClient.hasType('User');
      expect(hasType).toBe(true);
    });

    await test.step('Verify User has id and email fields', async () => {
      const type = await schemaClient.getType('User');
      const fieldNames = type?.fields?.map((f) => f.name) || [];

      expect(fieldNames).toContain('id');
      expect(fieldNames).toContain('email');
    });
  });
});

// ============================================================================
// Test Suite 5: Breaking Change Detection
// ============================================================================

test.describe('GraphQL Schema - Breaking Changes', () => {
  /**
   * This test compares current schema against a baseline.
   * The baseline is established from the first snapshot run.
   *
   * To update baseline after intentional breaking changes:
   * npm run e2e -- --update-snapshots --grep "Breaking Changes"
   */
  test('should detect no breaking changes from baseline', async ({ schemaClient }) => {
    const currentSchema = await schemaClient.getNormalizedSchema();

    await test.step('Compare against baseline snapshot', async () => {
      // This snapshot acts as our baseline for breaking change detection
      // If this test fails, it means the schema has changed
      expect(JSON.stringify(currentSchema.queryFields, null, 2)).toMatchSnapshot(
        'baseline-query-fields.json'
      );
      expect(JSON.stringify(currentSchema.mutationFields, null, 2)).toMatchSnapshot(
        'baseline-mutation-fields.json'
      );
    });

    await test.step('Verify no types were removed', async () => {
      // Create a baseline from constants (minimal expected types)
      const baselineTypes = [
        ...SCHEMA.AUTH_MODULE_TYPES,
        ...SCHEMA.FAMILY_MODULE_TYPES,
        ...SCHEMA.ERROR_TYPES,
      ];

      const currentTypes = Object.keys(currentSchema.types);

      for (const expectedType of baselineTypes) {
        expect(currentTypes, `Type '${expectedType}' was removed (breaking change)`).toContain(
          expectedType
        );
      }
    });
  });

  test('should demonstrate breaking change detection helper', async ({ schemaClient }) => {
    const currentSchema = await schemaClient.getNormalizedSchema();

    await test.step('Create mock baseline with extra field', async () => {
      // Simulate a baseline that has a field that was "removed"
      const mockBaseline: NormalizedSchema = {
        ...currentSchema,
        queryFields: [...currentSchema.queryFields, 'removedField'],
      };

      const changes = detectBreakingChanges(mockBaseline, currentSchema);

      // Should detect the "removed" field as a breaking change
      expect(changes.length).toBe(1);
      expect(changes[0].type).toBe('REMOVED_FIELD');
      expect(changes[0].path).toBe('Query.removedField');
    });
  });
});

// ============================================================================
// Test Suite 6: Performance Tests
// ============================================================================

test.describe('GraphQL Schema - Performance', () => {
  // Clear cache before each performance test for accurate measurements
  test.beforeEach(async ({ schemaClient }) => {
    schemaClient.clearCache();
  });

  test('introspection should complete within timeout', async ({ schemaClient }) => {
    await test.step('Measure introspection time', async () => {
      const start = performance.now();
      await schemaClient.fetchSchema(SCHEMA.INTROSPECTION_TIMEOUT);
      const duration = performance.now() - start;

      // Should complete well within timeout
      expect(duration).toBeLessThan(SCHEMA.INTROSPECTION_TIMEOUT);

      // Use Playwright annotations instead of console.log for better CI integration
      test.info().annotations.push({
        type: 'performance',
        description: `Introspection completed in ${duration.toFixed(2)}ms`,
      });

      // Warn if approaching threshold
      if (duration > SCHEMA.PERFORMANCE_WARNING_MS) {
        test.info().annotations.push({
          type: 'warning',
          description: `Introspection time (${duration.toFixed(2)}ms) exceeds warning threshold (${SCHEMA.PERFORMANCE_WARNING_MS}ms)`,
        });
      }
    });
  });

  test('schema size should be within acceptable bounds', async ({ schemaClient }) => {
    await test.step('Measure schema size', async () => {
      const sizeKB = await schemaClient.getSchemaSizeKB();

      // Should be within size limit
      expect(sizeKB).toBeLessThan(SCHEMA.MAX_SCHEMA_SIZE_KB);

      // Use Playwright annotation for schema size metric
      test.info().annotations.push({
        type: 'metric',
        description: `Schema size: ${sizeKB.toFixed(2)} KB`,
      });
    });
  });

  test('schema caching should work', async ({ schemaClient }) => {
    await test.step('First fetch should populate cache', async () => {
      const start1 = performance.now();
      await schemaClient.fetchSchema();
      const duration1 = performance.now() - start1;

      // Second fetch should be instant (from cache)
      const start2 = performance.now();
      await schemaClient.fetchSchema();
      const duration2 = performance.now() - start2;

      // Cached fetch should be essentially instant (< 5ms)
      // Using absolute threshold instead of ratio to avoid flaky tests on slow CI
      expect(duration2).toBeLessThan(5);

      test.info().annotations.push({
        type: 'performance',
        description: `First fetch: ${duration1.toFixed(2)}ms, Cached fetch: ${duration2.toFixed(2)}ms`,
      });
    });
  });
});

// ============================================================================
// Test Suite 7: Schema Metadata Tests
// ============================================================================

test.describe('GraphQL Schema - Metadata', () => {
  test('should have expected directives', async ({ schemaClient }) => {
    const schema = await schemaClient.getRawSchema();

    await test.step('Verify standard directives exist', async () => {
      const directiveNames = schema.directives.map((d) => d.name);

      // Standard GraphQL directives
      expect(directiveNames).toContain('skip');
      expect(directiveNames).toContain('include');
      expect(directiveNames).toContain('deprecated');
    });
  });

  test('should not have unexpected deprecations', async ({ schemaClient }) => {
    const schema = await schemaClient.getRawSchema();

    await test.step('Check for deprecated fields in domain types', async () => {
      const deprecatedFields: string[] = [];

      for (const type of schema.types) {
        // Skip built-in and Hot Chocolate types
        if (type.name.startsWith('__')) continue;
        if (type.name.includes('Connection') || type.name.includes('Edge')) continue;

        if (type.fields) {
          for (const field of type.fields) {
            if (field.isDeprecated) {
              deprecatedFields.push(`${type.name}.${field.name}: ${field.deprecationReason}`);
            }
          }
        }
      }

      // Use Playwright annotation instead of console.log
      if (deprecatedFields.length > 0) {
        test.info().annotations.push({
          type: 'info',
          description: `Deprecated fields found: ${deprecatedFields.join(', ')}`,
        });
      }

      // Snapshot deprecations to track intentional deprecations
      expect(JSON.stringify(deprecatedFields, null, 2)).toMatchSnapshot(
        'graphql-schema-deprecations.json'
      );
    });
  });

  test('error types should have message field', async ({ schemaClient }) => {
    await test.step('Verify error types have message field', async () => {
      for (const errorTypeName of SCHEMA.ERROR_TYPES) {
        const type = await schemaClient.getType(errorTypeName);

        // Explicit existence check - don't silently skip missing types
        expect(type, `Error type '${errorTypeName}' does not exist in schema`).toBeDefined();
        expect(type?.fields, `Error type '${errorTypeName}' has no fields`).toBeDefined();

        if (type && type.fields) {
          const fieldNames = type.fields.map((f) => f.name);
          expect(fieldNames, `Error type '${errorTypeName}' should have 'message' field`).toContain(
            'message'
          );
        }
      }
    });
  });
});
