import { spawnSync } from 'child_process';
import * as fs from 'fs';
import * as path from 'path';

/**
 * Global Teardown for Playwright Tests
 *
 * Cleans up test infrastructure:
 * 1. Stop .NET API process - SKIPPED IN CI
 * 2. Stop Docker Compose services - SKIPPED IN CI
 * 3. Remove temporary files
 *
 * CI Environment:
 * - GitHub Actions manages service lifecycle
 * - Skip Docker Compose and API cleanup when CI=true
 *
 * Local Development:
 * - Stops .NET API process
 * - Stops Docker Compose services
 */
async function globalTeardown() {
  console.log('\n🧹 Cleaning up test infrastructure...\n');

  const isCI = process.env.CI === 'true';

  if (isCI) {
    console.log('🔧 Running in CI environment - skipping cleanup');
    console.log('   GitHub Actions will handle service cleanup\n');
  } else {
    // Step 1: Stop .NET API (local development only)
    const pidFile = path.resolve(__dirname, '.test-api-pid');
    if (fs.existsSync(pidFile)) {
      try {
        const pid = fs.readFileSync(pidFile, 'utf-8').trim();
        console.log(`🛑 Stopping .NET API (PID: ${pid})...`);

        // Try graceful shutdown first
        process.kill(parseInt(pid, 10), 'SIGTERM');

        // Wait a bit for graceful shutdown
        await new Promise((resolve) => setTimeout(resolve, 2000));

        // Force kill if still running
        try {
          process.kill(parseInt(pid, 10), 'SIGKILL');
        } catch {
          // Process already stopped
        }

        fs.unlinkSync(pidFile);
        console.log('✅ .NET API stopped');
      } catch (error) {
        console.warn('⚠️  Could not stop .NET API:', error);
        // Non-fatal - continue with cleanup
      }
    }

    // Step 2: Stop Docker Compose services (local development only)
    const dockerComposeFile = path.resolve(
      __dirname,
      '../../../infrastructure/docker/docker-compose.yml'
    );

    if (fs.existsSync(dockerComposeFile)) {
      console.log('📦 Stopping Docker Compose services...');
      const result = spawnSync(
        'docker-compose',
        ['-f', dockerComposeFile, 'down', '-v'],
        { stdio: 'inherit' }
      );

      if (result.error) {
        console.warn('⚠️  Could not stop Docker Compose services:', result.error);
        // Non-fatal - continue with cleanup
      } else {
        console.log('✅ Docker services stopped');
      }
    }
  }

  // Step 3: Clean up temporary files
  const tempFiles = [
    path.resolve(__dirname, '.test-containers'),
    path.resolve(__dirname, '.test-api-pid'),
  ];

  for (const file of tempFiles) {
    if (fs.existsSync(file)) {
      try {
        fs.unlinkSync(file);
      } catch (error) {
        console.warn(`⚠️  Could not delete ${file}:`, error);
      }
    }
  }

  console.log('\n✅ Cleanup complete!\n');
}

export default globalTeardown;
