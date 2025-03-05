using System.Xml.Serialization;

namespace ApiTips.Api.Models.Brain;

public class PlayerSeated
{
    [XmlAttribute] public int seat { get; set; }
    [XmlAttribute] public string name { get; set; }
    [XmlAttribute] public int stack { get; set; }
}