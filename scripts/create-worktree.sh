#!/usr/bin/env bash
set -euo pipefail

VALID_TYPES=("feature" "fix" "refactor" "docs" "chore")
WORKTREE_BASE="../family2-worktrees"

usage() {
  cat <<EOF
Usage: $(basename "$0") <type> <issue-number> <description>

  type:         $(IFS='|'; echo "${VALID_TYPES[*]}")
  issue-number: GitHub issue number (digits only)
  description:  Brief kebab-case description

Example:
  $(basename "$0") feature 125 add-meal-planning

Creates:
  Branch:   feature/125-add-meal-planning
  Worktree: ${WORKTREE_BASE}/feature/125-add-meal-planning
EOF
  exit 1
}

error() {
  echo "Error: $1" >&2
  exit 1
}

# --- Validate argument count ---
if [[ $# -ne 3 ]]; then
  usage
fi

TYPE="$1"
ISSUE="$2"
DESCRIPTION="$3"

# --- Validate type ---
type_valid=false
for valid in "${VALID_TYPES[@]}"; do
  if [[ "$TYPE" == "$valid" ]]; then
    type_valid=true
    break
  fi
done
if [[ "$type_valid" == false ]]; then
  error "Invalid type '${TYPE}'. Must be one of: $(IFS=', '; echo "${VALID_TYPES[*]}")"
fi

# --- Validate issue number ---
if ! [[ "$ISSUE" =~ ^[0-9]+$ ]]; then
  error "Issue number must be numeric, got '${ISSUE}'"
fi

# --- Validate description (kebab-case) ---
if ! [[ "$DESCRIPTION" =~ ^[a-z0-9]+(-[a-z0-9]+)*$ ]]; then
  error "Description must be kebab-case (lowercase letters, digits, hyphens), got '${DESCRIPTION}'"
fi

# --- Construct names ---
BRANCH="${TYPE}/${ISSUE}-${DESCRIPTION}"
REPO_ROOT="$(git rev-parse --show-toplevel)"
WORKTREE_PATH="$(cd "$REPO_ROOT" && realpath -m "${WORKTREE_BASE}/${TYPE}/${ISSUE}-${DESCRIPTION}")"

# --- Check branch doesn't already exist ---
if git show-ref --verify --quiet "refs/heads/${BRANCH}"; then
  error "Branch '${BRANCH}' already exists"
fi

# --- Check worktree path doesn't already exist ---
if [[ -d "$WORKTREE_PATH" ]]; then
  error "Worktree directory already exists: ${WORKTREE_PATH}"
fi

# --- Create parent directory ---
mkdir -p "$(dirname "$WORKTREE_PATH")"

# --- Create worktree with new branch ---
# Bypass the reference-transaction hook that blocks branch switches
FAMILYHUB_ALLOW_CHECKOUT=1 git worktree add -b "$BRANCH" "$WORKTREE_PATH"

# Configure hooks path in the new worktree
git -C "$WORKTREE_PATH" config core.hooksPath .githooks

echo ""
echo "Worktree created successfully:"
echo "  Branch:   ${BRANCH}"
echo "  Path:     ${WORKTREE_PATH}"
echo ""
echo "To start working:"
echo "  cd ${WORKTREE_PATH}"
echo "  task up"
echo ""
echo "To remove later:"
echo "  git worktree remove ${WORKTREE_PATH}"
