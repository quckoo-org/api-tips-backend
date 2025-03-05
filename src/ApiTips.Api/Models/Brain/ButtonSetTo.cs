using System.Xml.Serialization;

namespace ApiTips.Api.Models.Brain;

public class ButtonSetTo
{
    [XmlAttribute] public int seat { get; set; } = 1;
}