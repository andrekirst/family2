#!/bin/bash

# GitHub Labels Setup for Family Hub
# Run: ./scripts/create-labels.sh

REPO="andrekirst/family2"

echo "Creating GitHub labels for $REPO..."

# Function to create or update label
create_label() {
  local name=$1
  local description=$2
  local color=$3

  gh label create "$name" \
    --description "$description" \
    --color "$color" \
    --repo "$REPO" \
    --force
}

# Type Labels (varied colors)
create_label "type-feature" "New feature or enhancement" "1d76db"
create_label "type-bug" "Bug or defect" "d73a4a"
create_label "type-epic" "Phase deliverable or epic" "5319e7"
create_label "type-research" "Research, spike, or investigation" "0e8a16"
create_label "type-docs" "Documentation work" "0075ca"
create_label "type-tech-debt" "Technical debt or refactoring" "fbca04"
create_label "type-infrastructure" "DevOps, Kubernetes, CI/CD" "bfd4f2"
create_label "type-security" "Security-related work" "d93f0b"
create_label "type-performance" "Performance optimization" "ff6347"

# Phase Labels (gradient)
create_label "phase-0" "Foundation & Tooling" "d4c5f9"
create_label "phase-1" "Core MVP" "c2e0c6"
create_label "phase-2" "Health Integration & Event Chains" "bfdadc"
create_label "phase-3" "Meal Planning & Finance" "c5def5"
create_label "phase-4" "Recurrence & Advanced Features" "f9d0c4"
create_label "phase-5" "Microservices Extraction" "fef2c0"
create_label "phase-6" "Mobile App & Extended Features" "e99695"
create_label "phase-7-future" "Future work (deferred)" "ededed"

# Service Labels (varied colors)
create_label "service-auth" "Auth Service" "0e8a16"
create_label "service-calendar" "Calendar Service" "1d76db"
create_label "service-task" "Task Service" "fbca04"
create_label "service-shopping" "Shopping Service" "d4c5f9"
create_label "service-health" "Health Service" "c2e0c6"
create_label "service-meal" "Meal Planning Service" "bfdadc"
create_label "service-finance" "Finance Service" "c5def5"
create_label "service-communication" "Communication Service" "f9d0c4"
create_label "service-frontend" "Frontend (Angular)" "e99695"
create_label "service-infrastructure" "Infrastructure/DevOps" "bfd4f2"
create_label "service-multiple" "Multiple services affected" "ededed"

# Status Labels (workflow)
create_label "status-triage" "Needs triage/review" "fbca04"
create_label "status-planning" "Planning phase" "fef2c0"
create_label "status-ready" "Ready for development" "0e8a16"
create_label "status-in-progress" "Actively being worked on" "1d76db"
create_label "status-blocked" "Blocked by dependency" "d93f0b"
create_label "status-review" "In code review" "d4c5f9"
create_label "status-testing" "In testing phase" "c2e0c6"
create_label "status-done" "Completed" "28a745"
create_label "status-wontfix" "Will not be implemented" "ededed"

# Priority Labels (red gradient)
create_label "priority-p0" "Critical - Must have for MVP" "b60205"
create_label "priority-p1" "High - Should have" "d93f0b"
create_label "priority-p2" "Medium - Nice to have" "fbca04"
create_label "priority-p3" "Low - Future consideration" "fef2c0"

# Domain Labels (teal/blue)
create_label "domain-auth" "Authentication & Authorization" "006b75"
create_label "domain-calendar" "Calendar & Events" "0e8a16"
create_label "domain-tasks" "Tasks & Chores" "1d76db"
create_label "domain-shopping" "Shopping & Lists" "5319e7"
create_label "domain-health" "Health Tracking" "d73a4a"
create_label "domain-meals" "Meal Planning" "fbca04"
create_label "domain-finance" "Finance & Budgeting" "0075ca"
create_label "domain-notifications" "Communication & Notifications" "f9d0c4"
create_label "domain-event-chains" "Event Chain Automation" "b60205"
create_label "domain-mobile" "Mobile Experience" "e99695"
create_label "domain-ux" "UX/UI Design" "d4c5f9"

# Effort Labels (gradient)
create_label "effort-xs" "< 1 day" "e4e669"
create_label "effort-s" "1-3 days" "c2e0c6"
create_label "effort-m" "1 week" "bfdadc"
create_label "effort-l" "2 weeks" "c5def5"
create_label "effort-xl" "> 2 weeks" "d4c5f9"

# Special Labels
create_label "good-first-issue" "Good for newcomers" "7057ff"
create_label "help-wanted" "Community help welcome" "008672"
create_label "breaking-change" "Breaking API change" "d93f0b"
create_label "needs-documentation" "Requires documentation" "0075ca"
create_label "needs-design" "Requires UX/UI design" "d4c5f9"
create_label "ai-assisted" "Claude Code will help implement" "5319e7"

echo "✅ Labels created successfully!"
