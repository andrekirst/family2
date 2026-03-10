import type { CodegenConfig } from '@graphql-codegen/cli';

const config: CodegenConfig = {
  schema: '../../FamilyHub.Api/schema.graphql',
  documents: 'src/**/*.graphql',
  generates: {
    'src/app/core/graphql/generated/': {
      preset: 'near-operation-file',
      presetConfig: {
        extension: '.generated.ts',
        baseTypesPath: 'types.ts',
      },
      plugins: ['typescript', 'typescript-operations', 'typescript-apollo-angular'],
      config: {
        addExplicitOverride: true,
        strictScalars: true,
      },
    },
  },
};

export default config;
