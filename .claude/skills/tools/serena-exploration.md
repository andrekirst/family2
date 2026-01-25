---
name: serena-exploration
description: Explore codebase using Serena symbolic tools
category: tools
module-aware: true
inputs:
  - module: Target module to explore (optional)
  - query: What you're looking for
---

# Serena Codebase Exploration

Workflow for efficiently exploring code using Serena's symbolic navigation.

## Context Loading

If module specified, load: `agent-os/profiles/modules/{module}.yaml`

## Step 1: Identify Entry Point

**If you know the file:**

```
get_symbols_overview(relative_path="path/to/file.cs", depth=1)
```

**If searching by pattern:**

```
search_for_pattern(
    substring_pattern="pattern",
    restrict_search_to_code_files=True,
    relative_path="src/api/Modules/{Module}"  # Restrict to module if known
)
```

**If searching by symbol name:**

```
find_symbol(
    name_path_pattern="SymbolName",
    substring_matching=True,
    relative_path="src/api/Modules/{Module}"  # Restrict if known
)
```

## Step 2: Navigate Symbol Tree

**For classes/modules (get structure):**

```
find_symbol(
    name_path_pattern="ClassName",
    depth=1,
    include_body=False
)
```

**For specific methods (get implementation):**

```
find_symbol(
    name_path_pattern="ClassName/MethodName",
    include_body=True
)
```

## Step 3: Trace Dependencies

**Find who uses this symbol:**

```
find_referencing_symbols(
    name_path="SymbolName",
    relative_path="path/to/file.cs"
)
```

**For event chain tracing:**

1. Find event: `find_symbol(name_path_pattern="EventName")`
2. Find handlers: `search_for_pattern(substring_pattern="INotificationHandler<EventName>")`

## Step 4: Reflect

```
think_about_collected_information()
```

**Ask yourself:**

- Is the information sufficient for the task?
- Are there gaps in understanding?
- Should I continue exploration or proceed?

## Common Exploration Scenarios

### Find Command Handler Implementation

```
1. find_symbol(name_path_pattern="CommandName", substring_matching=True)
2. find_symbol(name_path_pattern="CommandNameHandler", include_body=True)
```

### Find GraphQL Mutation Flow

```
1. search_for_pattern(substring_pattern="MutationName", paths_include_glob="**/Presentation/**")
2. find_symbol on Input DTO
3. find_symbol on Command
4. find_symbol on Handler
```

### Understand Module Structure

```
1. get_symbols_overview(relative_path="src/api/Modules/{Module}/Domain")
2. get_symbols_overview(relative_path="src/api/Modules/{Module}/Application")
3. get_symbols_overview(relative_path="src/api/Modules/{Module}/Presentation")
```

## Verification

- [ ] Started with overview, not full file read
- [ ] Used `include_body=False` until implementation details needed
- [ ] Restricted `relative_path` when module/directory known
- [ ] Called `think_about_collected_information` after exploration
- [ ] Documented findings for next steps
