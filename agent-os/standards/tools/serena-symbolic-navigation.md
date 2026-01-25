# Serena Symbolic Navigation

Pattern for exploring and understanding code using Serena's symbolic tools.

## Why

Serena's symbolic tools provide token-efficient code exploration by navigating
symbols (classes, methods, functions) rather than reading entire files. This
is critical for large codebases where context limits matter.

## When to Use

- **Always** for code exploration over standard Read/Grep tools
- Finding implementations of patterns across modules
- Understanding relationships between symbols
- Tracing event chains through the codebase

## Pattern: Symbol Discovery Chain

### Step 1: Overview First

```
get_symbols_overview(relative_path="path/to/file.cs", depth=1)
```

Get high-level view of symbols in a file without reading bodies.

### Step 2: Find Specific Symbols

```
find_symbol(name_path_pattern="ClassName/MethodName", include_body=False)
```

Locate symbols by name pattern. Use `depth=1` to see children.

### Step 3: Read Bodies When Needed

```
find_symbol(name_path_pattern="ClassName/MethodName", include_body=True)
```

Only read body when you need implementation details.

### Step 4: Trace References

```
find_referencing_symbols(name_path="ClassName/MethodName", relative_path="file.cs")
```

Find all code that references a symbol.

## Pattern: Cross-Module Discovery

For finding patterns across DDD modules:

1. Start with `search_for_pattern` to find candidates
2. Use `find_symbol` with `relative_path` restricted to module
3. Use `find_referencing_symbols` to trace dependencies

## Pattern: Event Chain Tracing

For understanding event-driven flows:

1. Find event class: `find_symbol(name_path_pattern="EventName")`
2. Find publishers: `find_referencing_symbols` on the event
3. Find handlers: `search_for_pattern(substring_pattern="INotificationHandler<EventName>")`
4. Trace handler implementations with `find_symbol`

## Rules

- Never read entire files unless strictly necessary
- Always start with `get_symbols_overview` for new files
- Use `include_body=False` until you need implementation details
- Restrict `relative_path` when you know the module/directory
- Use `substring_matching=True` when symbol name is uncertain
- Call `think_about_collected_information` after exploration sequences
