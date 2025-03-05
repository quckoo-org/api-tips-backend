using System.Xml.Serialization;

namespace ApiTips.Api.Models.Brain;

public class Version
{
    [XmlText]
    public string VersionNumber { get; set; }
}

#region Brain



// public class BrainService(IConfiguration configuration, ILogger<BrainService> logger) : BackgroundService
// {
//     private readonly string _serverHost = "195.211.166.9";
//     private readonly int _serverPort = 13031;
//     private TcpClient? _client;
//     private NetworkStream? _stream;
//
//    
//     
//
//
//   
//   
//  
//  
//  
//
//     private List<PlayerSeated> GetSeated()
//     {
//         var seated = new List<PlayerSeated>();
//         
//         var player = new PlayerSeated
//         {
//             seat = 1,
//             name = "XmlBot-0",
//             stack = 15000
//         };
//         seated.Add(player);
//         var player_2 = new PlayerSeated
//         {
//             seat = 2,
//             name = "HUPNLOpponent7Base-1",
//             stack = 15000
//         };
//         seated.Add(player_2);
//         return seated;
//     }
//     
//    
//
//     private List<BlindPosted> GetBlinds()
//     {
//         var blinds = new List<BlindPosted>();
//             // <BlindPosted name="XmlBot-0" blindType="SB" amount="50"/>
//             // <BlindPosted name="HUPNLOpponent7Base-1" blindType="BB" amount="100"/>
//             var blind = new BlindPosted
//             {
//                 amount = 50,
//                 name = "XmlBot-0",
//                 blindType = "SB"
//             };
//             var blind2 = new BlindPosted
//             {
//                 amount = 100,
//                 name = "HUPNLOpponent7Base-1",
//                 blindType = "SB"
//             };
//         blinds.Add(blind);
//         blinds.Add(blind2);
//         
//         return blinds;
//     }
//     
//     
//     public class StringWriterUtf8 : StringWriter
//     {
//         public override Encoding Encoding => Encoding.UTF8;
//     }
//
//     private string PrepareObjectToXml(Type type, object obj)
//     {
//         XmlSerializer serializer = new XmlSerializer(type);
//         XmlSerializerNamespaces ns = new XmlSerializerNamespaces();
//         ns.Add("", ""); // Отключаем дополнительные пространства имен
//
//         XmlWriterSettings settings = new XmlWriterSettings 
//         { 
//             OmitXmlDeclaration = true, 
//             Indent = false
//         };
//
//         using (StringWriter writer = new StringWriterUtf8()) // <-- Используем кастомный StringWriter
//         using (XmlWriter xmlWriter = XmlWriter.Create(writer, settings))
//         {
//             serializer.Serialize(xmlWriter, obj, ns);
//             
//             var xml = writer.ToString();
//             int index = xml.IndexOf("?>");
//             if (index != -1)
//             {
//                 xml = xml.Substring(index + 2).Trim();
//             }
//
//             return xml;
//         }
//     }
//     protected override async Task ExecuteAsync(CancellationToken stoppingToken)
//     {
//         while (!stoppingToken.IsCancellationRequested)
//         {
//             try
//             {
//                 #region Запрос для получения версии
//                 Version version = new Version { VersionNumber = "2" };
//                 var strVersion = PrepareObjectToXml(typeof(Version), version);
//                 Console.WriteLine(strVersion);
//                 #endregion
//
//                 #region Запрос для отправку событий
//                 // Отправка событий
//                 var ge = new GameEvents
//                 {
//                     GameStarted = new GameStarted(),
//                     GameParametersSetTo = new GameParametersSetTo(),
//                     ButtonSetTo = new ButtonSetTo(),
//                     PlayerSeated = GetSeated(),
//                     BlindPosted = GetBlinds(),
//                     StageStarted = new StageStarted(),
//                     HandDealt = new HandDealt()
//                 };
//                 var strGameEvents = PrepareObjectToXml(typeof(GameEvents), ge);
//                 Console.WriteLine(strGameEvents);
//                 #endregion
//
//                 #region Запрос на получение ответа
//                 var answerModel = new GetAnswer();
//                 var strAnswerModel = PrepareObjectToXml(typeof(GetAnswer), answerModel);
//                 Console.WriteLine(strAnswerModel);
//                 #endregion
//                 
//                 _client = new TcpClient();
//                 await _client.ConnectAsync(_serverHost, _serverPort, stoppingToken);
//                 _stream = _client.GetStream();
//
//                 logger.LogInformation("Подключение установлено!");
//
//                 // Отправка версии
//                 await SendMessageAsync(strVersion);
//                 var response = await ReadMessageAsync(stoppingToken);
//                 Console.WriteLine(response);
//                 
//               
//                 // Отправка событий
//                 await SendMessageAsync(strGameEvents);
//
//                 // Отправка запроса на получение ответа
//                 await SendMessageAsync(strAnswerModel);
//
//                 int cnt = 0;
//                 
//                 while (!stoppingToken.IsCancellationRequested)
//                 {
//
//                     Console.WriteLine("Новый цикл");
//                     Console.WriteLine("Отправлен Ping");
//                     await SendMessageAsync("<Ping/>");
//                     response = await ReadMessageAsync(stoppingToken);
//
//                     Console.WriteLine($"Ответ мозга: {response}");
//                     await Task.Delay(1000);
//                    
//
//                     // Читаем ответ
//                     if (response == "<Ping/>")
//                         await SendMessageAsync("<Pong/>");
//
//
//                     // Ждем 2 секунд перед следующей отправкой
//                     await Task.Delay(TimeSpan.FromSeconds(2), stoppingToken);
//                 }
//             }
//             catch (Exception ex)
//             {
//                 logger.LogInformation("Ошибка клиента: {Exception}", ex.Message);
//             }
//             finally
//             {
//                 _client?.Close();
//             }
//
//             await Task.Delay(5_000, stoppingToken);
//         }
//     }
//
//     private async Task SendMessageAsync(string message)
//     {
//         if (_stream == null || !_client!.Connected) return;
//
//         var xmlBytes = Encoding.UTF8.GetBytes(message + "\r\n");
//         var lengthPrefix = BitConverter.GetBytes(xmlBytes.Length);
//         // big-endian
//         Array.Reverse(lengthPrefix);
//
//         var packet = new byte[lengthPrefix.Length + xmlBytes.Length];
//         Array.Copy(lengthPrefix, 0, packet, 0, lengthPrefix.Length);
//         Array.Copy(xmlBytes, 0, packet, lengthPrefix.Length, xmlBytes.Length);
//
//         await _stream.WriteAsync(packet, 0, packet.Length);
//         await _stream.FlushAsync();
//     }
//
//     private async Task<string> ReadMessageAsync(CancellationToken stoppingToken)
//     {
//         if (_stream == null || !_client!.Connected) return "";
//
//         // Читаем префикс длины
//         var lengthPrefix = new byte[4];
//         _ = await _stream.ReadAsync(lengthPrefix.AsMemory(0, 4), stoppingToken);
//         Array.Reverse(lengthPrefix);
//         Console.WriteLine($"{lengthPrefix[0]} {lengthPrefix[1]}");
//         var messageLength = BitConverter.ToInt32(lengthPrefix, 0);
//
//         // Читаем тело XML
//         var messageBytes = new byte[messageLength];
//         _ = await _stream.ReadAsync(messageBytes.AsMemory(0, messageLength), stoppingToken);
//         return Encoding.UTF8.GetString(messageBytes);
//     }
// }
#endregion
