using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace ApiTips.Dal.schemas.system;

[Table(nameof(Permission), Schema = "system")]
[Index(nameof(Name), IsUnique = true)]
[Comment("Объект - разрешение")]
public class Permission
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    [ConcurrencyCheck]
    [Comment("Уникальный идентификатор разрешения")]
    public long Id { get; init; }

    [Required]
    [ConcurrencyCheck]
    [StringLength(255)]
    [Comment("Уникальное название разрешения")]
    public required string Name { get; set; }

    [ConcurrencyCheck]
    [Comment("Связь с ролями")]
    public List<Role> Roles { get; set; } = [];

    [ConcurrencyCheck]
    [Comment("Список методов для разрешения")]
    public List<Method> Methods { get; set; } = [];
}