using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace ApiTips.Dal.schemas.system;

[Table(nameof(Method), Schema = "system")]
[Index(nameof(Name), IsUnique = true)]
[Comment("Объект - метод")]
public class Method
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    [ConcurrencyCheck]
    [Comment("Уникальный идентификатор метода")]
    public long Id { get; init; }

    [ConcurrencyCheck]
    [Required]
    [StringLength(255)]
    [Comment("Уникальное название метода")]
    public required string Name { get; set; }

    [ConcurrencyCheck]
    [Comment("Связь с разрешениями")]
    public List<Permission> Permissions { get; set; } = [];
}