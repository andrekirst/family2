using System.Text;
using FamilyHub.SharedKernel.Interfaces;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Polly;
using Polly.Retry;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using RabbitMQ.Client.Exceptions;

namespace FamilyHub.Infrastructure.Messaging;

/// <summary>
/// RabbitMQ publisher implementation with connection management, retry policies,
/// and dead letter queue support.
/// </summary>
/// <remarks>
/// <para><strong>Features:</strong></para>
/// <list type="bullet">
/// <item>Thread-safe connection and channel management</item>
/// <item>Polly v8 resilience pipeline with exponential backoff and jitter</item>
/// <item>Dead Letter Exchange (DLX) for failed message routing</item>
/// <item>Publisher confirms for guaranteed delivery</item>
/// <item>Automatic recovery on connection failure</item>
/// </list>
/// <para><strong>Retry Strategy:</strong></para>
/// <para>
/// Uses exponential backoff with jitter to prevent thundering herd.
/// Default: 3 retries with 1s base delay.
/// </para>
/// </remarks>
public sealed partial class RabbitMqPublisher : IMessageBrokerPublisher, IAsyncDisposable
{
    private readonly ILogger<RabbitMqPublisher> _logger;
    private readonly RabbitMqSettings _settings;
    private readonly ResiliencePipeline _retryPipeline;
    private readonly SemaphoreSlim _connectionLock = new(1, 1);

    private IConnection? _connection;
    private IChannel? _channel;
    private bool _disposed;
    private bool _exchangesDeclared;

    /// <summary>
    /// Initializes a new instance of the <see cref="RabbitMqPublisher"/> class.
    /// </summary>
    /// <param name="logger">Logger for structured logging.</param>
    /// <param name="settings">RabbitMQ configuration settings.</param>
    public RabbitMqPublisher(
        ILogger<RabbitMqPublisher> logger,
        IOptions<RabbitMqSettings> settings)
    {
        _logger = logger;
        _settings = settings.Value;
        _retryPipeline = CreateRetryPipeline();
    }

    /// <summary>
    /// Creates the Polly v8 resilience pipeline with exponential backoff retry.
    /// </summary>
    private ResiliencePipeline CreateRetryPipeline()
    {
        return new ResiliencePipelineBuilder()
            .AddRetry(new RetryStrategyOptions
            {
                ShouldHandle = new PredicateBuilder()
                    .Handle<BrokerUnreachableException>()
                    .Handle<AlreadyClosedException>()
                    .Handle<OperationInterruptedException>()
                    .Handle<TimeoutException>()
                    .Handle<IOException>(),
                MaxRetryAttempts = _settings.MaxRetryAttempts,
                Delay = _settings.RetryBaseDelay,
                MaxDelay = _settings.RetryMaxDelay,
                BackoffType = DelayBackoffType.Exponential,
                UseJitter = true,
                OnRetry = args =>
                {
                    LogRetryAttempt(
                        args.AttemptNumber,
                        args.RetryDelay,
                        args.Outcome.Exception?.Message ?? "Unknown error");
                    return default;
                }
            })
            .Build();
    }

    /// <inheritdoc />
    public async Task PublishAsync(
        string exchange,
        string routingKey,
        string message,
        CancellationToken cancellationToken = default)
    {
        ObjectDisposedException.ThrowIf(_disposed, nameof(RabbitMqPublisher));
        ArgumentException.ThrowIfNullOrWhiteSpace(exchange);
        ArgumentException.ThrowIfNullOrWhiteSpace(routingKey);
        ArgumentException.ThrowIfNullOrWhiteSpace(message);

        await _retryPipeline.ExecuteAsync(async token =>
        {
            var channel = await EnsureChannelAsync(token);

            var body = Encoding.UTF8.GetBytes(message);
            var messageId = Guid.NewGuid().ToString();

            var properties = new BasicProperties
            {
                ContentType = "application/json",
                ContentEncoding = "utf-8",
                DeliveryMode = DeliveryModes.Persistent,
                Timestamp = new AmqpTimestamp(DateTimeOffset.UtcNow.ToUnixTimeSeconds()),
                MessageId = messageId
            };

            await channel.BasicPublishAsync(
                exchange: exchange,
                routingKey: routingKey,
                mandatory: false,
                basicProperties: properties,
                body: body,
                cancellationToken: token);

            LogMessagePublished(exchange, routingKey, messageId);
        }, cancellationToken);
    }

    /// <summary>
    /// Ensures a valid channel is available, creating connection if needed.
    /// Thread-safe connection/channel management using SemaphoreSlim.
    /// </summary>
    private async Task<IChannel> EnsureChannelAsync(CancellationToken cancellationToken)
    {
        if (_channel is { IsOpen: true })
        {
            return _channel;
        }

        await _connectionLock.WaitAsync(cancellationToken);
        try
        {
            // Double-check after acquiring lock
            if (_channel is { IsOpen: true })
            {
                return _channel;
            }

            // Close any stale connection/channel
            await CloseChannelAsync();
            await CloseConnectionAsync();

            // Create new connection
            var factory = new ConnectionFactory
            {
                HostName = _settings.Host,
                Port = _settings.Port,
                UserName = _settings.Username,
                Password = _settings.Password,
                VirtualHost = _settings.VirtualHost,
                RequestedConnectionTimeout = _settings.ConnectionTimeout
            };

            LogConnectionAttempt(_settings.Host, _settings.Port);

            _connection = await factory.CreateConnectionAsync(
                _settings.ClientProvidedName,
                cancellationToken);

            _connection.ConnectionShutdownAsync += OnConnectionShutdownAsync;

            // Create channel with publisher confirms if enabled
            var channelOptions = _settings.EnablePublisherConfirms
                ? new CreateChannelOptions(
                    publisherConfirmationsEnabled: true,
                    publisherConfirmationTrackingEnabled: true)
                : null;

            _channel = await _connection.CreateChannelAsync(
                options: channelOptions,
                cancellationToken: cancellationToken);

            // Declare exchanges (only once per connection)
            if (!_exchangesDeclared)
            {
                await DeclareExchangesAsync(_channel, cancellationToken);
                _exchangesDeclared = true;
            }

            LogConnectionEstablished(_settings.Host, _settings.Port);

            return _channel;
        }
        finally
        {
            _connectionLock.Release();
        }
    }

    /// <summary>
    /// Declares the main event exchange and dead letter exchange/queue.
    /// </summary>
    private async Task DeclareExchangesAsync(IChannel channel, CancellationToken cancellationToken)
    {
        // Declare dead letter exchange (fanout for catching all failed messages)
        await channel.ExchangeDeclareAsync(
            exchange: _settings.DeadLetterExchange,
            type: ExchangeType.Fanout,
            durable: true,
            autoDelete: false,
            arguments: null,
            noWait: false,
            cancellationToken: cancellationToken);

        // Declare dead letter queue
        await channel.QueueDeclareAsync(
            queue: _settings.DeadLetterQueue,
            durable: true,
            exclusive: false,
            autoDelete: false,
            arguments: null,
            noWait: false,
            cancellationToken: cancellationToken);

        // Bind DLQ to DLX (fanout doesn't need routing key, but we use "#" for clarity)
        await channel.QueueBindAsync(
            queue: _settings.DeadLetterQueue,
            exchange: _settings.DeadLetterExchange,
            routingKey: string.Empty,
            arguments: null,
            noWait: false,
            cancellationToken: cancellationToken);

        // Declare main event exchange (topic for flexible routing)
        // Note: Dead letter routing is configured per-queue, not per-exchange
        // The OutboxEventPublisher handles event-level retries before using DLQ
        await channel.ExchangeDeclareAsync(
            exchange: _settings.DefaultExchange,
            type: ExchangeType.Topic,
            durable: true,
            autoDelete: false,
            arguments: null,
            noWait: false,
            cancellationToken: cancellationToken);

        LogExchangesDeclared(
            _settings.DefaultExchange,
            _settings.DeadLetterExchange,
            _settings.DeadLetterQueue);
    }

    /// <summary>
    /// Handles connection shutdown events.
    /// </summary>
    private Task OnConnectionShutdownAsync(object sender, ShutdownEventArgs args)
    {
        LogConnectionShutdown(args.ReplyCode, args.ReplyText);
        _exchangesDeclared = false;
        return Task.CompletedTask;
    }

    private async Task CloseChannelAsync()
    {
        if (_channel != null)
        {
            try
            {
                if (_channel.IsOpen)
                {
                    await _channel.CloseAsync();
                }
            }
            catch (Exception ex)
            {
                LogChannelCloseError(ex.Message);
            }
            finally
            {
                _channel = null;
            }
        }
    }

    private async Task CloseConnectionAsync()
    {
        if (_connection != null)
        {
            try
            {
                _connection.ConnectionShutdownAsync -= OnConnectionShutdownAsync;
                if (_connection.IsOpen)
                {
                    await _connection.CloseAsync();
                }
            }
            catch (Exception ex)
            {
                LogConnectionCloseError(ex.Message);
            }
            finally
            {
                _connection = null;
            }
        }
    }

    /// <inheritdoc />
    public async ValueTask DisposeAsync()
    {
        if (_disposed)
        {
            return;
        }

        _disposed = true;

        await _connectionLock.WaitAsync();
        try
        {
            await CloseChannelAsync();
            await CloseConnectionAsync();
        }
        finally
        {
            _connectionLock.Release();
            _connectionLock.Dispose();
        }

        LogPublisherDisposed();
    }

    // High-performance structured logging using LoggerMessage attribute
    [LoggerMessage(LogLevel.Debug, "Attempting RabbitMQ connection to {Host}:{Port}")]
    partial void LogConnectionAttempt(string host, int port);

    [LoggerMessage(LogLevel.Information, "RabbitMQ connection established to {Host}:{Port}")]
    partial void LogConnectionEstablished(string host, int port);

    [LoggerMessage(LogLevel.Warning, "RabbitMQ connection shutdown: {ReplyCode} - {ReplyText}")]
    partial void LogConnectionShutdown(ushort replyCode, string replyText);

    [LoggerMessage(LogLevel.Debug, "Published message to {Exchange}/{RoutingKey} (MessageId: {MessageId})")]
    partial void LogMessagePublished(string exchange, string routingKey, string messageId);

    [LoggerMessage(LogLevel.Warning, "Retry attempt {AttemptNumber} after {Delay}. Error: {ErrorMessage}")]
    partial void LogRetryAttempt(int attemptNumber, TimeSpan delay, string errorMessage);

    [LoggerMessage(LogLevel.Information, "Declared exchanges: {MainExchange}, DLX: {DlxExchange}, DLQ: {DlqQueue}")]
    partial void LogExchangesDeclared(string mainExchange, string dlxExchange, string dlqQueue);

    [LoggerMessage(LogLevel.Warning, "Error closing RabbitMQ channel: {ErrorMessage}")]
    partial void LogChannelCloseError(string errorMessage);

    [LoggerMessage(LogLevel.Warning, "Error closing RabbitMQ connection: {ErrorMessage}")]
    partial void LogConnectionCloseError(string errorMessage);

    [LoggerMessage(LogLevel.Debug, "RabbitMQ publisher disposed")]
    partial void LogPublisherDisposed();
}
