# Serena Symbolic Editing

Pattern for modifying code using Serena's symbolic editing tools.

## Why

Symbolic editing is more precise and less error-prone than line-based editing.
It operates on semantic code units (methods, classes) rather than text positions.

## When to Use

- Replacing entire method/function bodies
- Adding new methods to classes
- Inserting new classes/functions in files
- Any edit that aligns with symbol boundaries

## When NOT to Use

- Single-line changes within a method
- Changes spanning partial symbol content
- Non-code files (use standard Edit tool)

## Pattern: Replace Symbol Body

```
replace_symbol_body(
    name_path="ClassName/MethodName",
    relative_path="path/to/file.cs",
    body="public void MethodName() { /* new implementation */ }"
)
```

**Important:** Body includes signature line but NOT docstrings/comments before it.

## Pattern: Insert After Symbol

Add new method after existing method:

```
insert_after_symbol(
    name_path="ClassName/ExistingMethod",
    relative_path="path/to/file.cs",
    body="\n    public void NewMethod() { }\n"
)
```

Use for:

- Adding methods to classes
- Adding new classes after existing ones
- Appending code at end of file (use last top-level symbol)

## Pattern: Insert Before Symbol

Add imports before first symbol:

```
insert_before_symbol(
    name_path="FirstClass",
    relative_path="path/to/file.cs",
    body="using NewNamespace;\n\n"
)
```

Use for:

- Adding import statements
- Adding file-level attributes/comments
- Inserting code at beginning of file

## Pattern: Safe Editing Workflow

1. **Read first**: `find_symbol(name_path, include_body=True)`
2. **Think**: `think_about_task_adherence()`
3. **Check references**: `find_referencing_symbols(name_path, relative_path)`
4. **Edit**: `replace_symbol_body()` or `insert_after_symbol()`
5. **Verify** (if uncertain): `find_symbol(name_path, include_body=True)`

## Rules

- Always call `think_about_task_adherence` before editing
- Retrieve symbol body first to understand current implementation
- Use `find_referencing_symbols` to check backward compatibility
- Verify results with `find_symbol` after editing (if uncertain)
- Never edit symbols without first reading them
- Prefer symbolic editing over line-based Edit tool for symbol-aligned changes
