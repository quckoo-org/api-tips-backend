using System.Net;
using ApiTips.Api.ServiceInterfaces;
using StackExchange.Redis;

namespace ApiTips.Api.Services;

public class RedisService : IRedisService
{
    private readonly IDatabaseAsync _dbDefault;
    private readonly ILogger<RedisService> _logger;
    private readonly ISubscriber _subscriber;

    public RedisService(ILogger<RedisService> logger, IConnectionMultiplexer multiplexer, IConfiguration configuration)
    {
        _logger = logger;

        multiplexer.GetServer(
            multiplexer.GetEndPoints().FirstOrDefault()
            ?? new IPEndPoint(IPAddress.Loopback, configuration.GetValue<int>("Redis:Port"))
        );
        _dbDefault = multiplexer.GetDatabase();
        _subscriber = multiplexer.GetSubscriber();
    }

    public async Task<string?> GetStringKeyAsync(string key, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(key))
        {
            _logger.LogError("The key [{Key}] must not be empty", key);
            return null;
        }

        try
        {
            // Проверяем токен отмены перед выполнением операции
            cancellationToken.ThrowIfCancellationRequested();

            // Передаём токен отмены в асинхронный метод, если он поддерживает его
            var keyDbValue = await _dbDefault.StringGetAsync(key).ConfigureAwait(false);

            if (keyDbValue.HasValue) return keyDbValue;

            _logger.LogWarning("Have no contains value with key {Key}", key);
        }
        catch (OperationCanceledException)
        {
            _logger.LogWarning("Operation was canceled for key [{Key}]", key);
            throw;
        }
        catch (Exception e)
        {
            _logger.LogCritical(
                "Couldn't get correct value | ExceptionMessage {ExceptionMessage} | ExceptionType {ExceptionType}",
                e.Message, e.GetType().Name);
        }

        return null;
    }

    public async Task<bool> SetKeyAsync(string key, string value, long duration = 12 * 60 * 60,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(key))
        {
            _logger.LogError("The key [{Key}] must not be empty", key);
            return false;
        }

        try
        {
            // Проверяем токен отмены
            cancellationToken.ThrowIfCancellationRequested();

            var keyDbValue = await _dbDefault.StringSetAsync(key, value, TimeSpan.FromSeconds(duration))
                .ConfigureAwait(false);

            if (keyDbValue) return true;

            _logger.LogWarning("Couldn't set {Value} with key {Key}", value, key);
        }
        catch (OperationCanceledException)
        {
            _logger.LogWarning("Operation was canceled while setting key [{Key}]", key);
            throw; // Пробрасываем исключение, если отмена должна быть обработана выше
        }
        catch (Exception e)
        {
            _logger.LogCritical(
                "Couldn't set key | ExceptionMessage {ExceptionMessage} | ExceptionType {ExceptionType}",
                e.Message, e.GetType().Name);
        }

        return false;
    }

    public async Task<bool> DeleteKeyAsync(string key, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(key))
        {
            _logger.LogError("The key [{Key}] must not be empty", key);
            return false;
        }

        try
        {
            // Проверяем токен отмены
            cancellationToken.ThrowIfCancellationRequested();

            // Удаляем ключ из Redis
            var isDeleted = await _dbDefault.KeyDeleteAsync(key).ConfigureAwait(false);

            if (isDeleted)
            {
                _logger.LogInformation("Key [{Key}] was successfully deleted", key);
                return true;
            }

            _logger.LogWarning("Key [{Key}] does not exist or could not be deleted", key);
        }
        catch (OperationCanceledException)
        {
            _logger.LogWarning("Operation was canceled while deleting key [{Key}]", key);
            throw;
        }
        catch (Exception e)
        {
            _logger.LogCritical(
                "Couldn't delete key | ExceptionMessage {ExceptionMessage} | ExceptionType {ExceptionType}",
                e.Message, e.GetType().Name);
        }

        return false;
    }

    public async Task<bool> SubscribeChannelAsync(string channelName, EventHandler<string>? eventHandler)
    {
        try
        {
            await _subscriber.SubscribeAsync(channelName, (_, value) =>
            {
                eventHandler?.Invoke(this, value!);
                _logger.LogInformation("Subscribe to channel {ChannelName} -  successfully", channelName);
            });
            return true;
        }
        catch (Exception e)
        {
            _logger.LogCritical(
                "Couldn't subscribe to channel {ChannelName} | ExceptionMessage {ExceptionMessage} | ExceptionType {ExceptionType}",
                channelName, e.Message, e.GetType().Name);
        }

        return false;
    }

    public async Task<bool> EmitEventAsync(string channelName, string data)
    {
        try
        {
            var publish = await _subscriber.PublishAsync(channelName, data);

            if (publish > 0)
            {
                _logger.LogInformation("Event [{Data}] has been emitted successfully in {Channel}", data,
                    channelName);
                return true;
            }

            _logger.LogWarning("Event [{Data}] has not been emitted in {Channel}", data, channelName);
        }
        catch (Exception e)
        {
            _logger.LogError(
                "Couldn't push data [{Data}] to channel {ChannelName} | ExceptionMessage {ExceptionMessage} | ExceptionType {ExceptionType}",
                data, channelName, e.Message, e.GetType().Name);
        }

        return false;
    }
}