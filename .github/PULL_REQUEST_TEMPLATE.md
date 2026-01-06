## Pull Request Description

### Summary
<!-- Brief description of what this PR does -->

### Related Issues
<!-- Link to related issues -->
Closes #
Related to #

### Type of Change
<!-- Mark relevant options with [x] -->
- [ ] Bug fix (non-breaking change which fixes an issue)
- [ ] New feature (non-breaking change which adds functionality)
- [ ] Breaking change (fix or feature that would cause existing functionality to not work as expected)
- [ ] Refactoring (no functional changes)
- [ ] Documentation update
- [ ] Infrastructure/DevOps change

---

## Architecture Impact

### Services Affected
<!-- Mark all that apply -->
- [ ] Auth Service
- [ ] Calendar Service
- [ ] Task Service
- [ ] Shopping Service
- [ ] Health Service
- [ ] Meal Planning Service
- [ ] Finance Service
- [ ] Communication Service
- [ ] Frontend (Angular)
- [ ] API Gateway
- [ ] Event Bus
- [ ] Infrastructure

### Domain Events
<!-- List any new domain events published or consumed -->

**Published:**

- `EventName` - Description

**Consumed:**

- `EventName` - Description

### Event Chain Impact
<!-- Does this PR affect any event chains? -->
- [ ] No event chain impact
- [ ] Modifies existing event chain: [Chain name]
- [ ] Adds new event chain: [Chain name]

### Database Changes
<!-- Mark if applicable -->
- [ ] Database schema changes (migration included)
- [ ] New tables/columns
- [ ] Data migration required

### API Changes
<!-- Mark if applicable -->
- [ ] GraphQL schema changes
- [ ] New queries/mutations
- [ ] Breaking API changes (requires version bump)

---

## Testing

### Test Coverage
<!-- Mark all that apply -->
- [ ] Unit tests added/updated
- [ ] Integration tests added/updated
- [ ] E2E tests added/updated
- [ ] Event chain tests added/updated
- [ ] Manual testing completed

### Testing Checklist
<!-- Verify these scenarios -->
- [ ] Happy path tested
- [ ] Error cases tested
- [ ] Edge cases considered
- [ ] Cross-service integration tested (if applicable)
- [ ] Event chain end-to-end tested (if applicable)

### Test Results
<!-- Paste relevant test output -->
```
# Paste test results here
```

---

## Quality Checks

### Code Quality

- [ ] Code follows project style guidelines
- [ ] No compiler warnings
- [ ] No linting errors
- [ ] Code is self-documenting or has appropriate comments
- [ ] DDD principles followed (bounded contexts respected)

### Security

- [ ] No sensitive data exposed (secrets, PII)
- [ ] Input validation implemented
- [ ] Authorization checks in place
- [ ] Security best practices followed

### Performance

- [ ] Performance impact considered
- [ ] No N+1 queries introduced
- [ ] Appropriate caching used
- [ ] Database queries optimized

---

## Documentation

### Documentation Updates
<!-- Mark all that apply -->
- [ ] Code comments added/updated
- [ ] GraphQL schema documentation updated
- [ ] API documentation updated
- [ ] README updated (if needed)
- [ ] `/docs/` updated (if needed)
- [ ] Changelog updated

### Documentation References
<!-- Link to relevant documentation -->
- Feature Backlog: `/docs/FEATURE_BACKLOG.md` - [Section]
- Implementation Roadmap: `/docs/implementation-roadmap.md` - [Phase]
- Domain Model: `/docs/domain-model-microservices-map.md` - [Service]

---

## Deployment

### Deployment Notes
<!-- Any special deployment considerations? -->
- [ ] Requires configuration changes
- [ ] Requires database migration
- [ ] Requires Kubernetes manifest updates
- [ ] Requires environment variable updates
- [ ] Can be deployed independently
- [ ] Requires coordinated deployment with other services

### Rollback Plan
<!-- How to rollback if issues occur? -->

---

## Screenshots / Videos
<!-- If UI changes, add screenshots or videos -->

---

## Checklist

### Before Merge

- [ ] PR title follows convention: `[Type] Brief description`
- [ ] All CI checks passing
- [ ] Code reviewed and approved
- [ ] Merge conflicts resolved
- [ ] Branch is up to date with main
- [ ] Documentation complete
- [ ] Tests passing (100% new code covered)

### Post-Merge

- [ ] Issue(s) closed
- [ ] Deployed to staging/production (if applicable)
- [ ] Monitoring dashboards checked
- [ ] Related documentation updated

---

## Additional Notes
<!-- Any additional context, concerns, or notes for reviewers -->
