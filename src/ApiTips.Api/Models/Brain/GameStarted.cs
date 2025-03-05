using System.Xml.Serialization;

namespace ApiTips.Api.Models.Brain;

public class GameStarted
{
    [XmlAttribute] 
    public int gameId { get; set; } = 12345;
    [XmlAttribute]
    public string pokerNetwork { get; set; } = "PS";
}