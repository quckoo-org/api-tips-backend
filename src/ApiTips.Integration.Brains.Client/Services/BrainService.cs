using System.Net.Sockets;
using System.Text;

namespace ApiTips.Integration.Brains.Client.Services;

public class BrainService(IConfiguration configuration, ILogger<BrainService> logger) : BackgroundService
{
    private readonly string _serverHost = configuration.GetValue<string>("Services:TcpServer:Host") ?? "127.0.0.1";
    private readonly int _serverPort = configuration.GetValue<int>("Services:TcpServer:Port");
    private TcpClient? _client;
    private NetworkStream? _stream;

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                _client = new TcpClient();
                await _client.ConnectAsync(_serverHost, _serverPort, stoppingToken);
                _stream = _client.GetStream();

                logger.LogInformation("Подключение установлено!");

                // Отправка версии
                await SendMessageAsync("<Version>2</Version>");

                while (!stoppingToken.IsCancellationRequested)
                {
                    // Отправляем Ping
                    await SendMessageAsync("<Ping/>");

                    // Читаем ответ
                    var response = await ReadMessageAsync(stoppingToken);
                    if (response == "<Pong/>") logger.LogInformation("Получен Pong, соединение активно");

                    // Ждем 10 секунд перед следующей отправкой
                    await Task.Delay(TimeSpan.FromSeconds(11), stoppingToken);
                }
            }
            catch (Exception ex)
            {
                logger.LogInformation("Ошибка клиента: {Exception}", ex.Message);
            }
            finally
            {
                _client?.Close();
            }

            await Task.Delay(5_000, stoppingToken);
        }
    }

    private async Task SendMessageAsync(string message)
    {
        if (_stream == null || !_client!.Connected) return;

        var xmlBytes = Encoding.UTF8.GetBytes(message + "\r\n");
        var lengthPrefix = BitConverter.GetBytes(xmlBytes.Length);
        // big-endian
        Array.Reverse(lengthPrefix);

        var packet = new byte[lengthPrefix.Length + xmlBytes.Length];
        Array.Copy(lengthPrefix, 0, packet, 0, lengthPrefix.Length);
        Array.Copy(xmlBytes, 0, packet, lengthPrefix.Length, xmlBytes.Length);

        await _stream.WriteAsync(packet, 0, packet.Length);
        await _stream.FlushAsync();
    }

    private async Task<string> ReadMessageAsync(CancellationToken stoppingToken)
    {
        if (_stream == null || !_client!.Connected) return "";

        // Читаем префикс длины
        var lengthPrefix = new byte[4];
        _ = await _stream.ReadAsync(lengthPrefix.AsMemory(0, 4), stoppingToken);
        Array.Reverse(lengthPrefix);
        var messageLength = BitConverter.ToInt32(lengthPrefix, 0);

        // Читаем тело XML
        var messageBytes = new byte[messageLength];
        _ = await _stream.ReadAsync(messageBytes.AsMemory(0, messageLength), stoppingToken);
        return Encoding.UTF8.GetString(messageBytes);
    }
}