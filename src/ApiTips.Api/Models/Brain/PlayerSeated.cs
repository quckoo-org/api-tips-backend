using System.Xml.Serialization;

namespace ApiTips.Api.Models.Brain;

/// <summary>
///     Посадка игрока за стол
/// </summary>
public class PlayerSeated
{
    /// <summary>
    ///     Номер места, за которое садится игрок. Целое число от 1 до 10.
    /// </summary>
    [XmlAttribute("seat")]
    public int Seat { get; set; }
    
    /// <summary>
    ///     Имя игрока, как оно задано в оригинальном источнике игры.
    /// </summary>
    [XmlAttribute("name")]
    public string Name { get; set; }
    
    /// <summary>
    ///     Размер стека игрока в фишках. Т.е. 100$ будет как `stack="100000"`
    /// </summary>
    [XmlAttribute("stack")]
    public int Stack { get; set; }
}