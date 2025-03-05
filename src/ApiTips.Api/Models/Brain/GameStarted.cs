using System.Xml.Serialization;

namespace ApiTips.Api.Models.Brain;

/// <summary>
///     Старт новой игры.
/// </summary>
public class GameStarted
{
    /// <summary>
    ///     Оригинальный номер раздачи. Необходимо
    /// </summary>
    [XmlAttribute("pokerNetwork")] 
    public int GameId { get; set; }
    
    /// <summary>
    ///     Код покерной сети.
    /// </summary>
    [XmlAttribute("pokerNetwork")]
    public string PokerNetwork { get; set; }
}