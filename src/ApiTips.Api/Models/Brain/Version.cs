using System.Xml.Serialization;

namespace ApiTips.Api.Models.Brain;

/// <summary>
///     Объект - модель для сериализации в XML-формат
/// </summary>
public class Version
{
    /// <summary>
    ///     Номер версии мозга, с которым производится работа - число
    /// </summary>
    [XmlText]
    public int VersionNumber { get; set; }
}