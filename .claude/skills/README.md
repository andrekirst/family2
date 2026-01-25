# Claude Code Skills

Task-specific patterns for Family Hub development, integrated with Agent OS.

## Available Skills

### Backend

| Skill | Description | Module-Aware |
|-------|-------------|--------------|
| `graphql-mutation` | Create GraphQL mutation with ADR-003 pattern | Yes |

### Frontend

| Skill | Description | Module-Aware |
|-------|-------------|--------------|
| `angular-component` | Create standalone Angular component | No |

### Database

| Skill | Description | Module-Aware |
|-------|-------------|--------------|
| `ef-migration` | Create EF Core migration with RLS | Yes |

### Testing

| Skill | Description | Module-Aware |
|-------|-------------|--------------|
| `playwright-test` | Create Playwright E2E test | Yes |

### Workflows

| Skill | Description | Module-Aware |
|-------|-------------|--------------|
| `feature-implementation` | Complete feature implementation workflow | Yes |

## Usage

Skills are invoked through Claude Code with parameters:

```
Invoke skill: backend/graphql-mutation
- mutationName: CreateFamily
- module: auth
- fields: [name: string]
```

## Module-Aware Skills

Skills marked as "Module-Aware" automatically load context from:

1. **Module Profile**: `agent-os/profiles/modules/{module}.yaml`
2. **Standards**: Relevant standards from `agent-os/standards/`
3. **Related Docs**: Documentation referenced in the profile

## Agent OS Integration

Skills work alongside Agent OS:

- **Standards** define patterns (what to follow)
- **Skills** provide step-by-step implementation (how to apply)
- **Profiles** provide context (where and what exists)

## Adding New Skills

1. Create skill file in appropriate category folder
2. Use frontmatter for metadata:

   ```yaml
   ---
   name: skill-name
   description: Brief description
   category: backend|frontend|database|testing|workflows
   module-aware: true|false
   inputs:
     - inputName: description
   ---
   ```

3. Update `meta/skill-manifest.json`

## Version

See `meta/skill-manifest.json` for current version and full skill list.
