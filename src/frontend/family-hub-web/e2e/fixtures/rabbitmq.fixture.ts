import { test as base } from '@playwright/test';
import amqp, { Connection, Channel, ConsumeMessage } from 'amqplib';
import { RABBITMQ, TIMEOUTS } from '../support/constants';

/**
 * RabbitMQ Fixture for Event Chain Verification
 *
 * Creates temporary queues bound to test exchange for event verification.
 * Allows tests to verify that domain events are published correctly.
 *
 * Usage:
 * ```typescript
 * import { test, expect } from '../fixtures/rabbitmq.fixture';
 *
 * test('should publish event', async ({ rabbitmq }) => {
 *   // Trigger action that publishes event
 *   await page.click('button');
 *
 *   // Verify event was published
 *   const messages = await rabbitmq.consumeMessages();
 *   const event = messages.find(m => m.eventType === 'FamilyCreated');
 *   expect(event).toBeDefined();
 * });
 * ```
 */

/**
 * Parsed event message structure
 */
export interface EventMessage {
  eventType: string;
  data: any;
  timestamp?: string;
  correlationId?: string;
  [key: string]: any;
}

/**
 * Type definitions for RabbitMQ fixtures
 */
export type RabbitMQFixture = {
  rabbitmq: {
    /** RabbitMQ connection */
    connection: Connection;

    /** RabbitMQ channel */
    channel: Channel;

    /** Name of the temporary queue */
    queueName: string;

    /**
     * Consume messages from the queue
     * Waits for RABBITMQ_CONSUME timeout then returns all received messages
     */
    consumeMessages: () => Promise<EventMessage[]>;

    /**
     * Consume a single message matching a predicate
     * @param predicate - Function to filter messages
     * @param timeout - Max wait time in ms (default: 5000)
     */
    waitForMessage: (
      predicate: (msg: EventMessage) => boolean,
      timeout?: number
    ) => Promise<EventMessage | null>;
  };
};

/**
 * Extend Playwright's test with RabbitMQ fixtures
 */
export const test = base.extend<RabbitMQFixture>({
  rabbitmq: async ({}, use) => {
    let connection: Connection;
    let channel: Channel;
    let queueName: string;

    await test.step('Setup RabbitMQ test queue', async () => {
      // Connect to RabbitMQ
      console.log('🐰 Connecting to RabbitMQ:', RABBITMQ.URL);
      connection = await amqp.connect(RABBITMQ.URL);
      channel = await connection.createChannel();

      // Assert test exchange exists (idempotent)
      await channel.assertExchange(
        RABBITMQ.TEST_EXCHANGE,
        RABBITMQ.EXCHANGE_TYPE,
        { durable: false }
      );

      // Create exclusive, auto-delete queue for this test
      const { queue } = await channel.assertQueue('', {
        exclusive: true,
        autoDelete: true,
      });
      queueName = queue;

      // Bind to all event types (#) on test exchange
      await channel.bindQueue(queueName, RABBITMQ.TEST_EXCHANGE, '#');

      console.log(`✅ RabbitMQ test queue created: ${queueName}`);
      console.log(`   Bound to exchange: ${RABBITMQ.TEST_EXCHANGE}`);
    });

    /**
     * Consume all messages from the queue
     * Returns after RABBITMQ_CONSUME timeout
     */
    const consumeMessages = async (): Promise<EventMessage[]> => {
      return new Promise((resolve) => {
        const messages: EventMessage[] = [];

        // Set up consumer
        channel.consume(
          queueName,
          (msg: ConsumeMessage | null) => {
            if (msg) {
              try {
                const content = msg.content.toString();
                const parsed = JSON.parse(content);
                messages.push(parsed);

                // Acknowledge message
                channel.ack(msg);

                console.log('📨 Received event:', parsed.eventType || 'unknown');
              } catch (error) {
                console.error('❌ Failed to parse message:', error);
                channel.nack(msg, false, false);
              }
            }
          },
          { noAck: false }
        );

        // Wait for messages to arrive, then resolve
        setTimeout(() => {
          console.log(`📬 Consumed ${messages.length} message(s) from queue`);
          resolve(messages);
        }, TIMEOUTS.RABBITMQ_CONSUME);
      });
    };

    /**
     * Wait for a specific message matching a predicate
     */
    const waitForMessage = async (
      predicate: (msg: EventMessage) => boolean,
      timeout: number = 5000
    ): Promise<EventMessage | null> => {
      return new Promise((resolve) => {
        let resolved = false;

        // Set up consumer
        const consumerTag = Math.random().toString(36).substring(7);
        channel.consume(
          queueName,
          (msg: ConsumeMessage | null) => {
            if (msg && !resolved) {
              try {
                const content = msg.content.toString();
                const parsed = JSON.parse(content);

                // Check if message matches predicate
                if (predicate(parsed)) {
                  resolved = true;
                  channel.ack(msg);
                  channel.cancel(consumerTag);
                  resolve(parsed);
                } else {
                  // Acknowledge but don't resolve
                  channel.ack(msg);
                }
              } catch (error) {
                console.error('❌ Failed to parse message:', error);
                channel.nack(msg, false, false);
              }
            }
          },
          { noAck: false, consumerTag }
        );

        // Timeout if no matching message found
        setTimeout(() => {
          if (!resolved) {
            channel.cancel(consumerTag);
            resolve(null);
          }
        }, timeout);
      });
    };

    // Provide fixture to test
    await use({
      connection,
      channel,
      queueName,
      consumeMessages,
      waitForMessage,
    });

    // Cleanup
    await test.step('Cleanup RabbitMQ test queue', async () => {
      try {
        await channel.deleteQueue(queueName);
        await channel.close();
        await connection.close();
        console.log('🧹 RabbitMQ test queue cleaned up');
      } catch (error) {
        console.error('❌ RabbitMQ cleanup error:', error);
        // Don't throw - cleanup failures shouldn't fail tests
      }
    });
  },
});

/**
 * Re-export expect from Playwright
 */
export { expect } from '@playwright/test';
