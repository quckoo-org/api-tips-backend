using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using ApiTips.Dal.schemas.system;
using Microsoft.EntityFrameworkCore;

namespace ApiTips.Dal.schemas.data;

[Table(nameof(Tariff), Schema = "data")]
[Index(nameof(Name), IsUnique = true)]
[Comment("Объект - тариф")]
public class Tariff
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    [ConcurrencyCheck]
    [Comment("Уникальный идентификатор тарифа")]
    public long Id { get; set; }

    [ConcurrencyCheck]
    [Required]
    [StringLength(255)]
    [Comment("Уникальное название тарифа")]
    public required string Name { get; set; }

    [ConcurrencyCheck]
    [Required]
    [StringLength(3)]
    [Comment("Валюта тарифа, ISO 4217")]
    public required string Currency { get; set; }

    [NotMapped]
    [Comment("Стоимость одной подсказки")]
    public decimal? TipPrice => PaidTipsCount is null or 0
        ? null
        : TotalPrice / PaidTipsCount.Value;

    [ConcurrencyCheck]
    [Comment("Количество бесплатных подсказок")]
    public long? FreeTipsCount { get; set; }

    [ConcurrencyCheck]
    [Comment("Количество оплаченных подсказок")]
    public long? PaidTipsCount { get; set; }

    [NotMapped]
    [Comment("Общее количество подсказок, вычисляемое поле")]
    public long? TotalTipsCount
    {
        get
        {
            var total = (FreeTipsCount ?? 0) + (PaidTipsCount ?? 0);
            return total == 0 ? null : total;
        }
    }

    [ConcurrencyCheck]
    [Required]
    [Comment("Общая стоимость тарифа, вводится менеджером")]
    public required decimal TotalPrice { get; set; }

    [ConcurrencyCheck]
    [Required]
    [Comment("Дата начала действия тарифа")]
    public required DateTime StartDateTime { get; set; }

    [ConcurrencyCheck]
    [Comment("Дата окончания действия тарифа")]
    public DateTime? EndDateTime { get; set; }

    [ConcurrencyCheck]
    [Comment("Дата сокрытия тарифа")]
    public DateTime? HideDateTime { get; set; }

    [ConcurrencyCheck]
    [Comment("Дата архивации тарифа")]
    public DateTime? ArchiveDateTime { get; set; }

    [ConcurrencyCheck]
    [Comment("Дата создания записи в БД по UTC")]
    public DateTime CreateDateTime { get; init; }

    [ConcurrencyCheck]
    [Comment("Автор создания тарифа")]
    public User? CreateBy { get; set; }
}