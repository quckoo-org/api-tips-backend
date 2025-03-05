using System.Xml.Serialization;

namespace ApiTips.Api.Models.Brain;

public class StageStarted
{
    [XmlAttribute]
    public string stage { get; set; } = "Preflop";
    [XmlAttribute]
    public string cards { get; set; } = "";
}