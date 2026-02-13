# Avatar Management - Shaping Decisions

## Scope

- Image upload with crop tool and auto-generated initials fallback
- 4 size variants: Tiny (24x24), Small (48x48), Medium (128x128), Large (512x512)
- Global avatar on User (Auth module) + optional per-family override on FamilyMember (Family module)
- REST endpoint for avatar serving with browser caching (ETag, Cache-Control)
- Frontend avatar display component usable everywhere (nav bar, members list, invitations)
- Frontend avatar upload component with crop tool (ngx-image-cropper)

## User Decisions

| Decision | Choice |
|----------|--------|
| Avatar types | Image upload + auto-generated initials fallback |
| Display scope | Everywhere (nav, members list, invitations, all user contexts) |
| Visibility | All family members see each other's avatars |
| Image processing | Full: crop tool, 4 size variants, format optimization |
| Storage | `IFileStorageService` abstraction + PostgreSQL large objects (Phase 1) |
| Size variants | Tiny (24x24), Small (48x48), Medium (128x128), Large (512x512) |
| Module ownership | Family module owns avatar management commands |
| Avatar scope | Global avatar on User (Auth) + optional per-family override on FamilyMember (Family) |
| Serving strategy | REST endpoint (not GraphQL) for browser caching and native `<img src>` support |
| Initials generation | Client-side (first letter of first + last name), colored background |
| Upload validation | MIME type (jpeg/png/webp), max 5MB, max 4096x4096, ImageSharp content verification |

## Architecture Decisions

- **File storage as infrastructure concern**: `IFileStorageService` lives in Common layer, not in any module. PostgreSQL large objects for Phase 1, swappable to S3/Azure Blob later.
- **Avatar aggregate in Common**: Avatar is cross-cutting (used by Auth's User and Family's FamilyMember), so the aggregate, repository, and EF config live in Common/Infrastructure/Avatar/.
- **REST for serving**: GraphQL is not ideal for binary content. REST provides native browser caching (ETag, Cache-Control), direct `<img src>` binding, and CDN compatibility.
- **Family module owns commands**: Upload/remove avatar mutations go through Family module because that's where user-facing family management lives. Domain events update Auth's User.AvatarId.
- **Per-family override**: FamilyMember can optionally have a different avatar than the global User avatar. Resolution: use FamilyMember.AvatarId if set, else fall back to User.AvatarId.

## Out of Scope

- Avatar moderation/approval workflow
- Animated GIF/video avatars
- AI-generated avatars
- Avatar history/versioning (only current avatar stored)
- S3/Azure Blob storage (Phase 1 uses PostgreSQL large objects)
- CDN integration (future optimization)
- Avatar for non-authenticated contexts (public profiles)
