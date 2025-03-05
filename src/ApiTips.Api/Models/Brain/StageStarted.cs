using System.Xml.Serialization;

namespace ApiTips.Api.Models.Brain;

/// <summary>
///     Старт очередной стадии игры.
/// </summary>
public class StageStarted
{
    /// <summary>
    ///     Стадия игры. Возможные значения
    ///         - Preflop
    ///         - Flop
    ///         - Turn
    ///         - River
    /// </summary>
    [XmlAttribute("stage")]
    public string Stage { get; set; }
    
    /// <summary>
    ///     Карты, выложенные на стол при старте очередной стадии игры.
    ///         - строка с картами в обычном виде, разделенными запятыми.
    /// </summary>
    [XmlAttribute("cards")]
    public string Cards { get; set; }
}