# k6 Performance Tests

Performance benchmarking suite for Family Hub GraphQL API using [k6](https://k6.io/).

## Requirements

- k6 installed (see installation below)
- Family Hub API running locally (or accessible endpoint)
- PostgreSQL and RabbitMQ for full API functionality

## Installation

### macOS

```bash
brew install k6
```

### Linux (Debian/Ubuntu)

```bash
sudo gpg -k
sudo gpg --no-default-keyring --keyring /usr/share/keyrings/k6-archive-keyring.gpg \
  --keyserver hkp://keyserver.ubuntu.com:80 --recv-keys C5AD17C747E3415A3642D57D77C6C491D6AC1D69
echo "deb [signed-by=/usr/share/keyrings/k6-archive-keyring.gpg] https://dl.k6.io/deb stable main" \
  | sudo tee /etc/apt/sources.list.d/k6.list
sudo apt-get update
sudo apt-get install k6
```

### Windows

```bash
choco install k6
# or
winget install k6 --source winget
```

### Docker

```bash
docker pull grafana/k6
```

## Quick Start

1. **Start the infrastructure:**

   ```bash
   cd infrastructure/docker
   docker-compose up -d
   ```

2. **Start the API:**

   ```bash
   cd src/api
   dotnet run --project FamilyHub.Api
   ```

3. **Run baseline test:**

   ```bash
   cd tests/performance
   k6 run scenarios/baseline.js
   ```

## Test Scenarios

### 1. Baseline (`scenarios/baseline.js`)

Establishes performance baseline under minimal load.

- **VUs:** 10 constant
- **Duration:** 1 minute
- **Purpose:** Quick validation, CI smoke test
- **Thresholds:** Strict (p95 < 150ms, error < 0.1%)

```bash
k6 run scenarios/baseline.js
```

### 2. Load (`scenarios/load.js`)

Tests system behavior under sustained, realistic load.

- **Stages:**
  - 0→50 VUs (1 min ramp-up)
  - 50 VUs (3 min sustained)
  - 50→100 VUs (1 min ramp-up)
  - 100 VUs (3 min sustained)
  - 100→0 VUs (1 min ramp-down)
- **Duration:** ~10 minutes total
- **Purpose:** Capacity validation, identify bottlenecks
- **Thresholds:** Default (p95 < 500ms, error < 1%)

```bash
k6 run scenarios/load.js
```

### 3. Stress (`scenarios/stress.js`)

Tests system limits and recovery under extreme load.

- **Stages:**
  - 10→200 VUs (30s spike)
  - 200 VUs (1 min peak)
  - 200→10 VUs (30s recovery)
  - 10 VUs (1 min stability check)
- **Duration:** ~3 minutes total
- **Purpose:** Find breaking point, verify graceful degradation
- **Thresholds:** Relaxed (p95 < 1000ms, error < 5%)

```bash
k6 run scenarios/stress.js
```

## Performance Targets

Based on [Section 12.7](../../docs/architecture/MODULAR-DOTNET-HOTCHOCOLATE-GUIDE.md) of the architecture guide:

| Metric | Baseline | Load | Stress |
|--------|----------|------|--------|
| p50 | < 50ms | < 200ms | < 500ms |
| p95 | < 150ms | < 500ms | < 1000ms |
| p99 | < 300ms | < 1000ms | < 3000ms |
| Error Rate | < 0.1% | < 1% | < 5% |

## Environment Configuration

Tests can target different environments:

```bash
# Local development (default)
k6 run scenarios/baseline.js

# Specify environment
k6 run -e K6_ENV=ci scenarios/baseline.js
k6 run -e K6_ENV=staging scenarios/baseline.js

# Custom GraphQL URL
k6 run -e GRAPHQL_URL=http://custom:5002/graphql scenarios/baseline.js
```

### Available Environments

| Environment | URL | Timeout |
|-------------|-----|---------|
| local | http://localhost:5002/graphql | 10s |
| ci | http://localhost:5002/graphql | 30s |
| staging | http://staging-api.familyhub.local/graphql | 30s |
| production | http://api.familyhub.local/graphql | 10s |

## Directory Structure

```
tests/performance/
├── README.md                 # This file
├── .gitignore               # Ignore results (except .gitkeep)
├── config/
│   ├── thresholds.js        # Threshold configurations
│   └── environments.js      # Environment-specific settings
├── helpers/
│   └── graphql.js           # GraphQL request helpers
├── scenarios/
│   ├── baseline.js          # Baseline test (10 VUs, 1 min)
│   ├── load.js              # Load test (0→100 VUs, 10 min)
│   └── stress.js            # Stress test (spike to 200 VUs)
└── results/                 # Test results (git-ignored)
    └── .gitkeep
```

## Test Results

Results are saved to `results/` directory:

- `{scenario}-{environment}-{timestamp}.json` - Timestamped results
- `{scenario}-{environment}-latest.json` - Latest run results

### Viewing Results

```bash
# Run with console output
k6 run scenarios/baseline.js

# Export to JSON
k6 run scenarios/baseline.js --summary-export=results/baseline.json

# Export to InfluxDB (for Grafana dashboards)
k6 run --out influxdb=http://localhost:8086/k6 scenarios/load.js
```

## CI/CD Integration

Performance tests run in GitHub Actions:

- **Trigger:** Manual (`workflow_dispatch`) or scheduled (nightly at 2 AM UTC)
- **Workflow:** `.github/workflows/performance.yml`

### Manual Trigger

1. Go to Actions → "Performance Tests (k6)"
2. Click "Run workflow"
3. Select scenario: `baseline`, `load`, `stress`, or `all`
4. Click "Run workflow"

### Results

- Artifacts uploaded to workflow run
- Summary displayed in GitHub Actions Summary
- Results retained for 30 days

## GraphQL Operations Tested

| Operation | Type | Auth Required | Purpose |
|-----------|------|---------------|---------|
| Health | Query | No | Infrastructure baseline |
| GetAuthUrl | Query | No | OAuth flow performance |

### Example Health Query

```graphql
query Health {
  health {
    status
    timestamp
    service
  }
}
```

## Writing New Tests

1. **Create scenario file in `scenarios/`:**

   ```javascript
   import { defaultThresholds } from '../config/thresholds.js';
   import { executeHealthQuery } from '../helpers/graphql.js';

   export const options = {
     vus: 10,
     duration: '1m',
     thresholds: defaultThresholds,
     tags: { scenario: 'my-test' },
   };

   export default function () {
     executeHealthQuery();
   }
   ```

2. **Add custom queries in `helpers/graphql.js`:**

   ```javascript
   export const queries = {
     // ... existing queries
     myQuery: `query MyQuery { ... }`,
   };
   ```

3. **Update this README** with new scenario documentation.

## Troubleshooting

### Connection Refused

Ensure API is running:

```bash
# Check if API is running
curl -sf http://localhost:5002/graphql -X POST \
  -H "Content-Type: application/json" \
  -d '{"query":"{ __typename }"}'

# Start API if not running
cd src/api && dotnet run --project FamilyHub.Api
```

### Slow Response Times

1. Check database connection:

   ```bash
   docker-compose ps  # Verify PostgreSQL is healthy
   ```

2. Check for N+1 queries in API logs
3. Review indexes in database

### Authentication Errors

For authenticated endpoints, set test token:

```bash
TEST_AUTH_TOKEN=your-token k6 run scenarios/load.js
```

### High Error Rates

1. Check API logs for exceptions
2. Verify RabbitMQ is running
3. Reduce VU count and verify
4. Check database connection pool limits

## Related Documentation

- **Architecture Guide:** [Section 12.7 - Performance Testing](../../docs/architecture/MODULAR-DOTNET-HOTCHOCOLATE-GUIDE.md)
- **Workflows:** [WORKFLOWS.md](../../docs/development/WORKFLOWS.md)
- **k6 Documentation:** https://k6.io/docs/

---

**Issue:** #63 - Create k6 Performance Benchmarking Suite
**Last Updated:** 2026-01-12
