using ApiTips.Dal.schemas.system;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;


namespace ApiTips.Dal.schemas.data;

[Table(nameof(Balance), Schema = "data")]
[Comment("Объект - баланс")]
public class Balance
{
    [Key, ForeignKey("User")]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    [ConcurrencyCheck]
    [Comment("Уникальный идентификатор баланса")]
    public long Id { get; set; }

    [ConcurrencyCheck]
    [Comment("Количество бесплатных подсказок")]
    public long FreeTipsCount { get; set; }

    [ConcurrencyCheck]
    [Comment("Количество оплаченных подсказок")]
    public long PaidTipsCount { get; set; }

    [NotMapped]
    [Comment("Общее количество подсказок, вычисляемое поле")]
    public long TotalTipsCount => FreeTipsCount + PaidTipsCount;

    [ConcurrencyCheck]
    [Required]
    [Comment("Пользователь - владелец баланса")]
    public required User User { get; set; }

    [ConcurrencyCheck]
    [Comment("Баланс, на изменение которого была направлена операция")]
    public List<BalanceHistory> History { get; set; } = [];
}
