using System.Xml.Serialization;

namespace ApiTips.Api.Models.Brain;

public class GetAnswer
{
    [XmlAttribute] public int commandSeq { get; set; } = 2;
    [XmlAttribute] public int gameSeq { get; set; } = 1;
    [XmlAttribute] public int gameId { get; set; } = 123;
}