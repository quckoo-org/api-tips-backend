using System.Xml.Serialization;

namespace ApiTips.Api.Models.Brain;

public class GameParametersSetTo
{
    [XmlAttribute("gameType")]
    public string GameType { get; set; } = "NL";
    
    [XmlAttribute("bigBlind")]
    public int BigBlind { get; set; } = 100;
    
    [XmlAttribute("ante")] 
    public int Ante { get; set; } = 0;
    
    [XmlAttribute("currency")] 
    public string currency { get; set; } = "USD";
    
    [XmlAttribute("gameDate")] 
    public string GameDate { get; set; } = "1741073097000";
}