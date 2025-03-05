using System.Xml.Serialization;

namespace ApiTips.Api.Models.Brain;

/// <summary>
///     Карты игрока
/// </summary>
public class HandDealt
{
    /// <summary>
    ///     Имя игрока, как оно задано в оригинальном источнике игры. 
    /// </summary>
    [XmlAttribute("name")]
    public string Name { get; set; }
    
    /// <summary>
    ///     Карты игрока. Это строка с картами в обчном виде, разделенными запятыми.
    /// </summary>
    [XmlAttribute("cards")]
    public string Cards { get; set; }
}