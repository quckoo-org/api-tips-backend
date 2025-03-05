using System.Xml.Serialization;

namespace ApiTips.Api.Models.Brain;

public class HandDealt
{
    [XmlAttribute]
    public string name { get; set; } = "XmlBot-0";
    [XmlAttribute]
    public string cards { get; set; } = "Th,Jd";
}