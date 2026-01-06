import { Injectable, computed, signal } from '@angular/core';
import { GraphQLService } from './graphql.service';
import { RoleMetadata, UserRole } from '../../features/family/models/family.models';

/**
 * Cache configuration for role metadata.
 */
const CACHE_KEY_ALL_ROLES = 'family-hub:roles:all';
const CACHE_KEY_INVITABLE_ROLES = 'family-hub:roles:invitable';
const CACHE_TTL_MS = 24 * 60 * 60 * 1000; // 24 hours

/**
 * Cached data structure with timestamp.
 */
interface CachedRoles {
  data: RoleMetadata[];
  timestamp: number;
}

/**
 * GraphQL query for fetching all available roles.
 */
const GET_AVAILABLE_ROLES = `
  query GetAvailableRoles {
    references {
      roles {
        all {
          value
          label
          description
          badgeColorClass
        }
      }
    }
  }
`;

/**
 * Service for managing user roles with LocalStorage caching.
 *
 * Features:
 * - Signal-based reactive state
 * - LocalStorage caching with 24-hour TTL
 * - Automatic cache expiration
 * - Force refresh capability
 * - Error handling and recovery
 *
 * Usage:
 * ```typescript
 * constructor(private roleService: RoleService) {}
 *
 * ngOnInit() {
 *   await this.roleService.loadRoles();
 *   const roles = this.roleService.allRoles();
 *   const invitable = this.roleService.invitableRoles();
 * }
 * ```
 */
@Injectable({
  providedIn: 'root'
})
export class RoleService {
  /**
   * All available roles (OWNER, ADMIN, MEMBER, MANAGED_ACCOUNT).
   * Signal for reactive updates.
   */
  readonly allRoles = signal<RoleMetadata[]>([]);

  /**
   * Loading state for async operations.
   */
  readonly isLoading = signal<boolean>(false);

  /**
   * Error state (null if no error).
   */
  readonly error = signal<string | null>(null);

  /**
   * Computed signal: Invitable roles (ADMIN, MEMBER only).
   * Excludes OWNER.
   */
  readonly invitableRoles = computed(() =>
    this.allRoles().filter(
      r => r.value !== 'OWNER'
    )
  );

  private graphql = inject(GraphQLService);

  /**
   * Loads all available roles from cache or API.
   *
   * @param forceRefresh - If true, bypasses cache and fetches fresh data
   * @returns Promise that resolves when roles are loaded
   */
  async loadRoles(forceRefresh = false): Promise<void> {
    try {
      this.isLoading.set(true);
      this.error.set(null);

      // Try cache first (unless force refresh)
      if (!forceRefresh) {
        const cachedRoles = this.getCachedRoles(CACHE_KEY_ALL_ROLES);
        if (cachedRoles) {
          this.allRoles.set(cachedRoles);
          this.isLoading.set(false);
          return;
        }
      }

      // Fetch from API
      const result = await this.graphql.query<{
        references: {
          roles: {
            all: RoleMetadata[]
          }
        }
      }>(GET_AVAILABLE_ROLES);

      if (result?.references?.roles?.all) {
        const roles = result.references.roles.all;
        this.allRoles.set(roles);
        this.setCachedRoles(CACHE_KEY_ALL_ROLES, roles);
      } else {
        throw new Error('No role data returned from API');
      }
    } catch (err) {
      const errorMessage = err instanceof Error ? err.message : 'Failed to load roles';
      this.error.set(errorMessage);
      console.error('RoleService: Failed to load roles', err);

      // Fall back to empty array on error
      this.allRoles.set([]);
    } finally {
      this.isLoading.set(false);
    }
  }

  /**
   * Gets metadata for a specific role.
   *
   * @param role - The role to get metadata for
   * @returns RoleMetadata object or undefined if not found
   */
  getRoleMetadata(role: UserRole): RoleMetadata | undefined {
    return this.allRoles().find(r => r.value === role);
  }

  /**
   * Clears all cached role data.
   * Useful for logout or cache invalidation.
   */
  clearCache(): void {
    localStorage.removeItem(CACHE_KEY_ALL_ROLES);
    localStorage.removeItem(CACHE_KEY_INVITABLE_ROLES);
  }

  /**
   * Gets cached roles from LocalStorage if not expired.
   *
   * @param key - Cache key to retrieve
   * @returns Cached roles or null if expired/missing
   */
  private getCachedRoles(key: string): RoleMetadata[] | null {
    try {
      const cached = localStorage.getItem(key);
      if (!cached) {
        return null;
      }

      const { data, timestamp }: CachedRoles = JSON.parse(cached);
      const now = Date.now();

      // Check if cache is expired
      if (now - timestamp > CACHE_TTL_MS) {
        localStorage.removeItem(key);
        return null;
      }

      return data;
    } catch (err) {
      console.error('RoleService: Failed to read cache', err);
      return null;
    }
  }

  /**
   * Stores roles in LocalStorage with current timestamp.
   *
   * @param key - Cache key to store under
   * @param roles - Role metadata to cache
   */
  private setCachedRoles(key: string, roles: RoleMetadata[]): void {
    try {
      const cached: CachedRoles = {
        data: roles,
        timestamp: Date.now()
      };
      localStorage.setItem(key, JSON.stringify(cached));
    } catch (err) {
      console.error('RoleService: Failed to write cache', err);
    }
  }
}
