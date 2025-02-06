using ApiTips.Dal.Enums;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;


namespace ApiTips.Dal.schemas.data;

[Table(nameof(BalanceHistory), Schema = "data")]
[Comment("Объект - изменение баланса")]
public class BalanceHistory
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    [ConcurrencyCheck]
    [Comment("Уникальный идентификатор записи истории изменения баланса")]
    public long Id { get; set; }

    [ConcurrencyCheck]
    [Comment("Величина изменения количества бесплатных подсказок")]
    public long? FreeTipsCountChangedTo { get; set; }

    [ConcurrencyCheck]
    [Comment("Величина изменения количества оплаченных подсказок")]
    public long? PaidTipsCountChangedTo { get; set; }

    [NotMapped]
    [Comment("Общая величина изменения количества подсказок, вычисляемое поле")]
    public long? TotalTipsCountChangedTo
    {
        get
        {
            var total = (FreeTipsCountChangedTo ?? 0) + (PaidTipsCountChangedTo ?? 0);
            return total == 0 ? null : total;
        }
    }

    [ConcurrencyCheck]
    [Required]
    [Comment("Тип операции (пополнение/списание)")]
    public required BalanceOperationType OperationType { get; set; }

    [ConcurrencyCheck]
    [Required]
    [Comment("Описание причины изменения баланса (пример: покупка/списание/промо)")]
    public required string ReasonDescription { get; set; }

    [ConcurrencyCheck]
    [Comment("Дата совершения операции")]
    public DateTime OperationDateTime { get; set; }

    [ConcurrencyCheck]
    [Required]
    [Comment("Количество подсказок на балансе после выполнения операции")]
    public required long TotalTipsBalance { get; set; }

    [ConcurrencyCheck]
    [Required]
    [Comment("Баланс, на изменение которого была направлена операция")]
    public required Balance Balance { get; set; }
}
