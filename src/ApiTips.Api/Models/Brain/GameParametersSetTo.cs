using System.Xml.Serialization;

namespace ApiTips.Api.Models.Brain;

public class GameParametersSetTo
{
    [XmlAttribute] public string gameType { get; set; } = "NL";
    [XmlAttribute] public int bigBlind { get; set; } = 100;
    [XmlAttribute] public int ante { get; set; } = 0;
    [XmlAttribute] public string currency { get; set; } = "USD";
    [XmlAttribute] public string gameDate { get; set; } = "1741073097000";
}