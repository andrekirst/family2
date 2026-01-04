import { defineConfig, devices } from '@playwright/test';

/**
 * Playwright Configuration for Family Hub E2E Tests
 *
 * Key Principles:
 * - Zero tolerance for flaky tests (retries: 0)
 * - Cross-browser testing (Chromium, Firefox, WebKit)
 * - Manual CI trigger only (optimized for local development)
 * - Real backend with Testcontainers
 * - Desktop-first (1280x720)
 *
 * @see https://playwright.dev/docs/test-configuration
 */
export default defineConfig({
  testDir: './e2e',

  /**
   * Zero tolerance for flaky tests
   * - fullyParallel: false (tests run sequentially due to shared backend)
   * - retries: 0 (forces fixing root causes immediately)
   * - workers: 1 (single worker for shared Testcontainers backend)
   */
  fullyParallel: false,
  retries: 0,
  workers: 1,

  /**
   * Reporter configuration
   * - html: Interactive HTML report for local debugging
   * - junit: XML report for CI/CD integration
   * - list: Console output for terminal feedback
   */
  reporter: [
    ['html', { open: 'never', outputFolder: 'playwright-report' }],
    ['junit', { outputFile: 'playwright-report/junit.xml' }],
    ['list']
  ],

  /**
   * Global test configuration
   */
  use: {
    // Base URL for page.goto('/path')
    baseURL: 'http://localhost:4200',

    // Trace settings - comprehensive debugging on failure
    trace: 'on-first-retry',
    screenshot: 'only-on-failure',
    video: 'retain-on-failure',

    // Playwright auto-waiting (no manual waits needed)
    actionTimeout: 10000,
    navigationTimeout: 30000,
  },

  /**
   * Test output directory
   * Contains traces, videos, screenshots
   */
  outputDir: 'playwright-results',

  /**
   * Projects for cross-browser testing
   * All browsers test at 1280x720 (desktop viewport)
   * Mobile viewports deferred to Phase 2
   */
  projects: [
    {
      name: 'chromium',
      use: {
        ...devices['Desktop Chrome'],
        viewport: { width: 1280, height: 720 },
      },
    },
    {
      name: 'firefox',
      use: {
        ...devices['Desktop Firefox'],
        viewport: { width: 1280, height: 720 },
      },
    },
    {
      name: 'webkit',
      use: {
        ...devices['Desktop Safari'],
        viewport: { width: 1280, height: 720 },
      },
    },
  ],

  /**
   * Global setup and teardown
   * Manages Testcontainers lifecycle (PostgreSQL, RabbitMQ, .NET API)
   */
  globalSetup: './e2e/global-setup.ts',
  globalTeardown: './e2e/global-teardown.ts',

  /**
   * Web server for local development
   * Starts Angular dev server if not already running
   * In CI, backend is started separately via Docker Compose
   */
  webServer: {
    command: 'npm run start',
    url: 'http://localhost:4200',
    reuseExistingServer: !process.env.CI,
    timeout: 120000,
  },
});
