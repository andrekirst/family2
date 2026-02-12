#!/usr/bin/env bash
# Convert a git branch name into a Docker-safe environment name.
# Usage: env-name.sh [branch-name]
# If no argument, reads current branch from git.
# Examples:
#   feature/121-sidebar  -> feature-121-sidebar
#   main                 -> main
#   fix/bug-123          -> fix-bug-123
set -euo pipefail

BRANCH="${1:-$(git rev-parse --abbrev-ref HEAD)}"

# Replace slashes and underscores with hyphens, lowercase, strip leading/trailing hyphens
echo "$BRANCH" | tr '/_' '-' | tr '[:upper:]' '[:lower:]' | sed 's/^-//;s/-$//'
