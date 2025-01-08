namespace ApiTips.Api.ServiceInterfaces;

public interface IRedisService
{
    /// <summary>
    ///     Получение значения по ключу
    /// </summary>
    Task<string?> GetStringKeyAsync(string key, CancellationToken cancellationToken = default);

    /// <summary>
    ///     Установка значения по ключу
    /// </summary>
    Task<bool> SetKeyAsync(string key, string value, long duration = 12 * 60 * 60,
        CancellationToken cancellationToken = default);

    /// <summary>
    ///     Подписка на канал
    /// </summary>
    Task<bool> SubscribeChannelAsync(string channelName, EventHandler<string>? eventHandler);

    /// <summary>
    ///     Отправка события в канал
    /// </summary>
    Task<bool> EmitEventAsync(string channelName, string data);
}