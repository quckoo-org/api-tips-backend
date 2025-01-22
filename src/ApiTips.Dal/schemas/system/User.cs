using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using ApiTips.Dal.schemas.data;
using Microsoft.EntityFrameworkCore;

namespace ApiTips.Dal.schemas.system;

[Table(nameof(User), Schema = "system")]
[Index(nameof(Email), IsUnique = true)]
[Comment("Объект - пользователь")]
public class User
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    [ConcurrencyCheck]
    [Comment("Уникальный идентификатор пользователя")]
    public long Id { get; init; }

    [Required]
    [ConcurrencyCheck]
    [StringLength(100)]
    [Comment("Уникальная почта пользователя")]
    public required string Email { get; set; }

    [ConcurrencyCheck]
    [StringLength(20)]
    [Comment("Телефон пользователя")]
    public string? PhoneNumber { get; set; }

    [Required]
    [ConcurrencyCheck]
    [StringLength(255)]
    [Comment("Пароль пользователя в зашифрованном виде (SHA256)")]
    public required string Password { get; set; }

    [Required]
    [ConcurrencyCheck]
    [StringLength(100)]
    [Comment("Имя пользователя")]
    public required string FirstName { get; set; }

    [ConcurrencyCheck]
    [StringLength(100)]
    [Comment("Отчество пользователя")]
    public string? SecondName { get; set; }

    [Required]
    [ConcurrencyCheck]
    [StringLength(255)]
    [Comment("Фамилия пользователя")]
    public required string LastName { get; set; }

    [Required]
    [ConcurrencyCheck]
    [StringLength(255)]
    [Comment("Код страны пользователя (ISO 3166-1 alpha-3)")]
    public required string Cca3 { get; set; }

    [ConcurrencyCheck]
    [Comment("Дата верификации записи в БД по UTC")]
    public DateTime? VerifyDateTime { get; set; }

    [ConcurrencyCheck]
    [Comment("Дата блокировки записи в БД по UTC")]
    public DateTime? LockDateTime { get; set; }

    [ConcurrencyCheck]
    [Comment("Дата удаления записи в БД по UTC")]
    public DateTime? DeleteDateTime { get; set; }

    [ConcurrencyCheck]
    [Comment("Дата создания записи в БД по UTC")]
    public DateTime CreateDateTime { get; init; }

    [ConcurrencyCheck]
    [Comment("Список ролей пользователя")]
    public List<Role> Roles { get; set; } = [];
    
    [ConcurrencyCheck]
    [ForeignKey("PaymentId")]
    [Comment("Платежная информация для пользователя")]
    public Payment? Payment { get; set; }
}