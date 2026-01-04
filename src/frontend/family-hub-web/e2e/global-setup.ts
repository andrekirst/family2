import { chromium, FullConfig } from '@playwright/test';
import { spawn, spawnSync, ChildProcess } from 'child_process';
import * as fs from 'fs';
import * as path from 'path';

/**
 * Global Setup for Playwright Tests
 *
 * Manages Testcontainers lifecycle:
 * 1. Start Docker Compose services (PostgreSQL, RabbitMQ) - SKIPPED IN CI
 * 2. Wait for service health checks
 * 3. Start .NET API in Test environment - SKIPPED IN CI
 * 4. Wait for GraphQL endpoint availability
 *
 * CI Environment:
 * - GitHub Actions provides postgres/rabbitmq via services
 * - API is started separately in CI workflow
 * - This setup skips Docker Compose and API startup when CI=true
 *
 * Local Development:
 * - Uses Docker Compose to start postgres/rabbitmq
 * - Starts .NET API via dotnet run
 */
async function globalSetup(config: FullConfig) {
  console.log('\n🚀 Starting test infrastructure...\n');

  const isCI = process.env.CI === 'true';

  if (isCI) {
    console.log('🔧 Running in CI environment - skipping Docker Compose setup');
    console.log('   Using GitHub Actions services for postgres/rabbitmq');
    console.log('   API should be started separately in CI workflow\n');
  } else {
    // Local development: Start Docker Compose services
    const dockerComposeFile = path.resolve(
      __dirname,
      '../../../infrastructure/docker/docker-compose.yml'
    );

    // Check if docker-compose file exists
    if (!fs.existsSync(dockerComposeFile)) {
      console.log('⚠️  Docker Compose file not found, assuming services already running');
      console.log(`   Expected: ${dockerComposeFile}`);
      return; // Non-fatal - services might be running already
    }

    // Step 1: Start infrastructure services (PostgreSQL, RabbitMQ)
    console.log('📦 Starting PostgreSQL and RabbitMQ...');
    const composeResult = spawnSync(
      'docker-compose',
      ['-f', dockerComposeFile, 'up', '-d', 'postgres', 'rabbitmq'],
      { stdio: 'inherit' }
    );

    if (composeResult.error) {
      console.error('❌ Failed to start Docker Compose services:', composeResult.error);
      throw composeResult.error;
    }

    // Step 2: Wait for services to be healthy
    console.log('\n⏳ Waiting for services to be ready...');

    // Wait for PostgreSQL
    await waitForService(
      'PostgreSQL',
      async () => {
        const result = spawnSync('docker', [
          'exec',
          'familyhub-postgres',
          'pg_isready',
          '-U',
          'familyhub',
        ]);
        return result.status === 0;
      },
      30000
    );

    // Wait for RabbitMQ
    await waitForService(
      'RabbitMQ',
      async () => {
        const result = spawnSync('docker', [
          'exec',
          'familyhub-rabbitmq',
          'rabbitmq-diagnostics',
          'ping',
        ]);
        return result.status === 0;
      },
      30000
    );

    console.log('✅ Infrastructure services ready!\n');
  }

  // Step 3: Start .NET API in Test environment (background process)
  // SKIP IN CI - API is started separately in GitHub Actions workflow
  if (!isCI) {
    console.log('🔧 Starting .NET API in Test environment...');

    const apiProjectPath = path.resolve(
      __dirname,
      '../../../api/FamilyHub.Api/FamilyHub.Api.csproj'
    );

    // Check if API project exists
    if (!fs.existsSync(apiProjectPath)) {
      console.log('⚠️  .NET API project not found, skipping API startup');
      console.log(`   Expected: ${apiProjectPath}`);
      return; // Non-fatal - might be testing frontend only
    }

    const apiProcess = spawn(
      'dotnet',
      ['run', '--project', apiProjectPath, '--environment', 'Test'],
      {
        detached: true,
        stdio: ['ignore', 'pipe', 'pipe'],
        env: {
          ...process.env,
          ASPNETCORE_ENVIRONMENT: 'Test',
          DOTNET_ENVIRONMENT: 'Test',
        },
      }
    );

    // Capture API output for debugging
    apiProcess.stdout?.on('data', (data) => {
      const output = data.toString().trim();
      if (output) console.log(`[API] ${output}`);
    });

    apiProcess.stderr?.on('data', (data) => {
      const output = data.toString().trim();
      if (output) console.error(`[API ERROR] ${output}`);
    });

    // Store PID for cleanup
    const pidFile = path.resolve(__dirname, '.test-api-pid');
    if (apiProcess.pid) {
      fs.writeFileSync(pidFile, apiProcess.pid.toString());
    }
  } else {
    console.log('🔧 Skipping API startup - running in CI (API started by workflow)\n');
  }

  // Step 4: Wait for GraphQL endpoint
  await waitForService(
    'GraphQL API',
    async () => {
      try {
        const browser = await chromium.launch({ headless: true });
        const page = await browser.newPage();
        const response = await page.goto('http://localhost:5002/graphql', {
          timeout: 5000,
          waitUntil: 'domcontentloaded',
        });
        await browser.close();
        return response?.status() === 200;
      } catch {
        return false;
      }
    },
    60000
  );

  console.log('✅ .NET API ready!\n');
  console.log('🎉 Test infrastructure fully initialized!\n');
}

/**
 * Wait for a service to become available
 */
async function waitForService(
  serviceName: string,
  checkFn: () => Promise<boolean>,
  timeout: number
): Promise<void> {
  const startTime = Date.now();
  const interval = 1000; // Check every second

  while (Date.now() - startTime < timeout) {
    try {
      const isReady = await checkFn();
      if (isReady) {
        console.log(`✅ ${serviceName} is ready`);
        return;
      }
    } catch (error) {
      // Service not ready yet, continue waiting
    }

    // Wait before next check
    await new Promise((resolve) => setTimeout(resolve, interval));
    process.stdout.write('.');
  }

  throw new Error(`❌ ${serviceName} failed to start within ${timeout}ms`);
}

export default globalSetup;
