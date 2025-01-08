using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace ApiTips.Dal.schemas.system;

[Table(nameof(Role), Schema = "system")]
[Index(nameof(Name), IsUnique = true)]
[Comment("Объект - пользователь")]
public class Role
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    [ConcurrencyCheck]
    [Comment("Уникальный идентификатор роли")]
    public long Id { get; init; }

    [Required]
    [ConcurrencyCheck]
    [StringLength(255)]
    [Comment("Уникальное название роли")]
    public required string Name { get; set; }

    [ConcurrencyCheck]
    [Comment("Связь с пользователями")]
    public List<User> Users { get; set; } = [];
    
    [ConcurrencyCheck]
    [Comment("Список разрешений для роли")]
    public List<Permission> Permissions { get; set; } = [];
}