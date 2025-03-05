using System.Xml.Serialization;

namespace ApiTips.Api.Models.Brain;

public class BlindPosted
{
    [XmlAttribute("name")]
    public string Name { get; set; }
    [XmlAttribute("blindType")]
    public string BlindType { get; set; }
    [XmlAttribute("amount")]
    public int Amount { get; set; }
}