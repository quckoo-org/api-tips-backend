using System.Xml.Serialization;

namespace ApiTips.Api.Models.Brain;

public class GameEvents
{
        
    [XmlAttribute] public int commandSeq { get; set; } = 1;

    [XmlAttribute] public int gameSeq { get; set; } = 1;
    [XmlAttribute] public int gameId { get; set; } = 123;
    [XmlElement("GameStarted")] public GameStarted GameStarted { get; set; }
    [XmlElement("GameParametersSetTo")] public GameParametersSetTo GameParametersSetTo { get; set; }
    [XmlElement("ButtonSetTo")] public ButtonSetTo ButtonSetTo { get; set; }
    [XmlElement("PlayerSeated")] 
    public List<PlayerSeated> PlayerSeated { get; set; }
    [XmlElement("BlindPosted")] 
    public List<BlindPosted> BlindPosted { get; set; }
    [XmlElement("StageStarted")] public StageStarted StageStarted { get; set; }
    [XmlElement("HandDealt")] public HandDealt HandDealt { get; set; }
}