using System.Xml.Serialization;

namespace ApiTips.Api.Models.Brain;

public class ButtonSetTo
{
    [XmlAttribute("seat")] public int Seat { get; set; } = 1;
}