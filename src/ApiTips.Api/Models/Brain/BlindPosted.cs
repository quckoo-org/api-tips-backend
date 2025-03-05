using System.Xml.Serialization;

namespace ApiTips.Api.Models.Brain;

public class BlindPosted
{
    [XmlAttribute]
    public string name { get; set; }
    [XmlAttribute]
    public string blindType { get; set; }
    [XmlAttribute]
    public int amount { get; set; }
}