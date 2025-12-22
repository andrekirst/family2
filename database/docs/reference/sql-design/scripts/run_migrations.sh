#!/bin/bash
# ============================================================================
# Script: run_migrations.sh
# Description: Execute database migrations for Family Hub modules
# Usage: ./run_migrations.sh [module_name] [environment]
# Author: Database Administrator Agent (Claude Code)
# Date: 2025-12-22
# ============================================================================

set -e  # Exit on error
set -u  # Exit on undefined variable

# Colors for output
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
BLUE='\033[0;34m'
NC='\033[0m' # No Color

# Script directory
SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
DATABASE_DIR="$(dirname "$SCRIPT_DIR")"
MIGRATIONS_DIR="$DATABASE_DIR/migrations"

# Default values
MODULE="${1:-all}"
ENVIRONMENT="${2:-development}"

# Database configuration
DB_HOST="${FAMILY_HUB_DB_HOST:-localhost}"
DB_PORT="${FAMILY_HUB_DB_PORT:-5432}"
DB_USER="${FAMILY_HUB_DB_USER:-postgres}"
DB_PASSWORD="${FAMILY_HUB_DB_PASSWORD:-}"
DB_NAME="${FAMILY_HUB_DB_NAME:-family_hub_dev}"

# Construct connection string
if [ -n "$DB_PASSWORD" ]; then
    PGPASSWORD="$DB_PASSWORD"
    export PGPASSWORD
fi

# Functions
print_header() {
    echo -e "\n${BLUE}============================================================================${NC}"
    echo -e "${BLUE}$1${NC}"
    echo -e "${BLUE}============================================================================${NC}\n"
}

print_success() {
    echo -e "${GREEN}✓ $1${NC}"
}

print_error() {
    echo -e "${RED}✗ $1${NC}"
}

print_warning() {
    echo -e "${YELLOW}⚠ $1${NC}"
}

print_info() {
    echo -e "${BLUE}ℹ $1${NC}"
}

check_database_connection() {
    print_info "Checking database connection..."
    if psql -h "$DB_HOST" -p "$DB_PORT" -U "$DB_USER" -d "$DB_NAME" -c "SELECT 1" > /dev/null 2>&1; then
        print_success "Database connection successful"
        return 0
    else
        print_error "Cannot connect to database"
        print_info "Connection details:"
        echo "  Host: $DB_HOST"
        echo "  Port: $DB_PORT"
        echo "  User: $DB_USER"
        echo "  Database: $DB_NAME"
        return 1
    fi
}

run_migration_file() {
    local file=$1
    local filename=$(basename "$file")

    print_info "Running migration: $filename"

    if psql -h "$DB_HOST" -p "$DB_PORT" -U "$DB_USER" -d "$DB_NAME" -f "$file" > /dev/null 2>&1; then
        print_success "Migration completed: $filename"
        return 0
    else
        print_error "Migration failed: $filename"
        print_error "Check the SQL file for errors: $file"
        return 1
    fi
}

run_module_migrations() {
    local module=$1
    local module_dir="$MIGRATIONS_DIR/$module"

    if [ ! -d "$module_dir" ]; then
        print_error "Module directory not found: $module_dir"
        return 1
    fi

    print_header "Running migrations for module: $module"

    # Find all .sql files in the module directory (excluding seed data by default)
    local migration_files=("$module_dir"/*.sql)

    if [ ${#migration_files[@]} -eq 0 ]; then
        print_warning "No migration files found in $module_dir"
        return 0
    fi

    # Sort files to ensure correct execution order
    IFS=$'\n' migration_files=($(sort <<<"${migration_files[*]}"))
    unset IFS

    local total_migrations=${#migration_files[@]}
    local completed=0

    for file in "${migration_files[@]}"; do
        # Skip seed data in production
        if [[ "$ENVIRONMENT" == "production" && "$(basename "$file")" == *"seed_data"* ]]; then
            print_warning "Skipping seed data in production: $(basename "$file")"
            continue
        fi

        if run_migration_file "$file"; then
            ((completed++))
        else
            print_error "Migration failed. Aborting remaining migrations."
            return 1
        fi
    done

    print_success "Completed $completed/$total_migrations migrations for module: $module"
}

run_all_migrations() {
    print_header "Running migrations for all modules"

    # Get list of module directories
    local modules=($(ls -d "$MIGRATIONS_DIR"/*/ 2>/dev/null | xargs -n 1 basename))

    if [ ${#modules[@]} -eq 0 ]; then
        print_warning "No modules found in $MIGRATIONS_DIR"
        return 0
    fi

    print_info "Found ${#modules[@]} module(s): ${modules[*]}"

    for module in "${modules[@]}"; do
        if ! run_module_migrations "$module"; then
            print_error "Failed to run migrations for module: $module"
            return 1
        fi
    done

    print_success "All migrations completed successfully"
}

verify_migrations() {
    print_header "Verifying database schema"

    print_info "Checking schemas..."
    psql -h "$DB_HOST" -p "$DB_PORT" -U "$DB_USER" -d "$DB_NAME" -c "
        SELECT schema_name
        FROM information_schema.schemata
        WHERE schema_name NOT IN ('pg_catalog', 'information_schema', 'pg_toast')
        ORDER BY schema_name;
    "

    print_info "Checking tables..."
    psql -h "$DB_HOST" -p "$DB_PORT" -U "$DB_USER" -d "$DB_NAME" -c "
        SELECT schemaname, tablename
        FROM pg_tables
        WHERE schemaname NOT IN ('pg_catalog', 'information_schema')
        ORDER BY schemaname, tablename;
    "

    print_success "Schema verification completed"
}

show_usage() {
    cat << EOF
Usage: $0 [module_name] [environment]

Arguments:
  module_name   Name of the module to migrate (default: all)
                Available modules:
                  - auth
                  - all (runs all modules)

  environment   Target environment (default: development)
                Options: development, staging, production

Environment Variables:
  FAMILY_HUB_DB_HOST      Database host (default: localhost)
  FAMILY_HUB_DB_PORT      Database port (default: 5432)
  FAMILY_HUB_DB_USER      Database user (default: postgres)
  FAMILY_HUB_DB_PASSWORD  Database password
  FAMILY_HUB_DB_NAME      Database name (default: family_hub_dev)

Examples:
  # Run all migrations
  $0

  # Run auth module migrations only
  $0 auth

  # Run auth module migrations for production
  $0 auth production

  # Run with custom database
  FAMILY_HUB_DB_NAME=family_hub_test $0 auth
EOF
}

# Main execution
main() {
    if [[ "${1:-}" == "-h" || "${1:-}" == "--help" ]]; then
        show_usage
        exit 0
    fi

    print_header "Family Hub Database Migration Tool"
    print_info "Module: $MODULE"
    print_info "Environment: $ENVIRONMENT"
    print_info "Database: $DB_NAME on $DB_HOST:$DB_PORT"

    # Check database connection
    if ! check_database_connection; then
        exit 1
    fi

    # Run migrations
    if [ "$MODULE" == "all" ]; then
        if ! run_all_migrations; then
            exit 1
        fi
    else
        if ! run_module_migrations "$MODULE"; then
            exit 1
        fi
    fi

    # Verify migrations
    verify_migrations

    print_header "Migration completed successfully!"
}

# Run main function
main "$@"
