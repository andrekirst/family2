# References for Universal Search and Command Palette

## Similar Implementations

### Widget Provider/Registry Pattern

- **Location**: `src/FamilyHub.Api/Common/Widgets/`
- **Relevance**: The search feature mirrors this pattern exactly. Widgets use `IWidgetProvider` → `IWidgetRegistry` → `WidgetRegistryInitializer` (IHostedService). Search will use `ISearchProvider` → `ISearchRegistry` → `SearchRegistryInitializer`.
- **Key patterns**:
  - `IWidgetProvider` returns static `WidgetDescriptor[]` — same for `ICommandPaletteProvider`
  - `WidgetRegistry` is a Singleton collecting descriptors at startup
  - `WidgetRegistryInitializer` iterates `IEnumerable<IWidgetProvider>` and registers all
  - Each module implements `IWidgetProvider` in its own namespace

### GraphQL Namespace Types

- **Location**: `src/FamilyHub.Api/Common/Infrastructure/GraphQL/NamespaceTypes/RootQuery.cs`
- **Relevance**: Shows how to add a new namespace type (`SearchQuery`) and wire it into the root query
- **Key patterns**:
  - Each namespace returns `new NamespaceType()` (e.g., `public FamilyQuery Family() => new()`)
  - `[Authorize]` attribute on namespace entry points
  - Query types extend namespace types via `[ExtendObjectType(typeof(SearchQuery))]`

### IModule Registration Pattern

- **Location**: `src/FamilyHub.Api/Common/Modules/IModule.cs`
- **Relevance**: `SearchModule` follows the standard module registration pattern
- **Key patterns**:
  - `public void Register(IServiceCollection services, IConfiguration configuration)`
  - Registered in `Program.cs` via `builder.Services.RegisterModule<SearchModule>(configuration)`
  - Self-contained DI — module registers its own services

### Family Module Search Reference

- **Location**: `src/FamilyHub.Api/Features/Family/`
- **Relevance**: First module to get search providers; shows repository patterns to follow
- **Key patterns**:
  - `IFamilyMemberRepository` for data access — will add `SearchByNameAsync` method
  - `FamilyModule.Register()` for adding new service registrations
  - `FamilyRole.GetPermissions()` for permission filtering of commands

## Visual References

- Dribbble command palette designs (centered modal, grouped results, keyboard navigation):
  - https://cdn.dribbble.com/userupload/35497075/file/original-0b58551936814d239aa32b67ec6aa9b4.png
  - https://cdn.dribbble.com/userupload/41560267/file/original-90b8712f20e8c684df02ecdae80b3daa.jpg
  - https://cdn.dribbble.com/userupload/12779867/file/original-1f4c7b529953b29d7e8a2727130d263c.jpg
  - https://cdn.dribbble.com/userupload/11284127/file/original-19187f94751fa67962d504aec9fd0520.png
