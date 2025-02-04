using System.Net;
using System.Net.Sockets;
using System.Text;

namespace ApiTips.Integration.Brains.Server.Services;

public class BrainService(IConfiguration configuration, ILogger<BrainService> logger) : BackgroundService
{
    private readonly int _port = configuration.GetValue<int>("App:Ports:Tcp");
    private TcpListener? _server;

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _server = new TcpListener(IPAddress.Any, _port);
        try
        {
            _server.Start();

            logger.LogInformation(
                "The application [{AppName}] is successfully started at [{StartTime}] (UTC) | protocol TCP | port : [{Port}]",
                AppDomain.CurrentDomain.FriendlyName,
                DateTime.UtcNow.ToString("F"),
                _port);

            while (!stoppingToken.IsCancellationRequested)
                try
                {
                    var client = await _server.AcceptTcpClientAsync(stoppingToken);
                    logger.LogInformation("Клиент подключен!");

                    _ = HandleClientAsync(client, stoppingToken); // Обработка клиента в фоновом потоке
                }
                catch (TaskCanceledException ex)
                {
                    logger.LogInformation("Отмена операции [{ExceptionType}] | {Message}",
                        nameof(TaskCanceledException), ex.Message);
                    break;
                }
                catch (OperationCanceledException ex)
                {
                    logger.LogInformation("Отмена операции [{ExceptionType}] | {Message}",
                        nameof(OperationCanceledException), ex.Message);
                    break;
                }
                catch (Exception ex)
                {
                    logger.LogError("Ошибка сервера: {Message}", ex.Message);
                }
        }
        catch (Exception ex)
        {
            logger.LogCritical("Критическая ошибка при запуске сервера: {Message}", ex.Message);
        }
        finally
        {
            if (_server.Server.IsBound)
            {
                _server.Stop();
                logger.LogInformation(
                    "The application [{AppName}] is successfully stopped at [{StartTime}] (UTC) | protocol TCP | port : [{Port}]",
                    AppDomain.CurrentDomain.FriendlyName,
                    DateTime.UtcNow.ToString("F"),
                    _port);
            }
        }
    }

    private async Task HandleClientAsync(TcpClient client, CancellationToken stoppingToken)
    {
        await using var stream = client.GetStream();

        while (!stoppingToken.IsCancellationRequested && client.Connected)
            try
            {
                // Читаем префикс длины (4 байта, big-endian)
                var lengthPrefix = new byte[4];
                var readBytes = await stream.ReadAsync(lengthPrefix.AsMemory(0, 4), stoppingToken);
                if (readBytes == 0) break; // Разрыв соединения

                Array.Reverse(lengthPrefix);
                var messageLength = BitConverter.ToInt32(lengthPrefix, 0);

                // Читаем тело XML
                var messageBytes = new byte[messageLength];
                readBytes = await stream.ReadAsync(messageBytes.AsMemory(0, messageLength), stoppingToken);
                if (readBytes == 0) break; // Разрыв соединения

                var message = Encoding.UTF8.GetString(messageBytes);
                logger.LogInformation("Получено: {Message}", message);

                // Если получен <Ping/>, отправляем <Pong/>
                if (message.Trim() == "<Ping/>")
                {
                    await SendMessageAsync(stream, "<Pong/>", stoppingToken);
                    logger.LogInformation("Ответил: <Pong/>");
                }
                // Если получена <Version>, отправляем <VersionOk/>
                else if (message.StartsWith("<Version>"))
                {
                    await SendMessageAsync(stream, "<VersionOk/>", stoppingToken);
                    logger.LogInformation("Ответил: <VersionOk/>");
                }
            }
            catch (Exception ex)
            {
                logger.LogInformation("Ошибка обработки клиента: {Message}", ex.Message);
                break;
            }

        client.Close();
        logger.LogInformation("Клиент отключен");
    }

    private async Task SendMessageAsync(NetworkStream stream, string response, CancellationToken stoppingToken)
    {
        var xmlBytes = Encoding.UTF8.GetBytes(response + "\r\n");
        var lengthPrefix = BitConverter.GetBytes(xmlBytes.Length);

        // big-endian
        Array.Reverse(lengthPrefix);

        var packet = new byte[lengthPrefix.Length + xmlBytes.Length];
        Array.Copy(lengthPrefix, 0, packet, 0, lengthPrefix.Length);
        Array.Copy(xmlBytes, 0, packet, lengthPrefix.Length, xmlBytes.Length);

        await stream.WriteAsync(packet, stoppingToken);
        await stream.FlushAsync(stoppingToken);
    }

    public override async Task StopAsync(CancellationToken stoppingToken)
    {
        await base.StopAsync(stoppingToken);
    }
}