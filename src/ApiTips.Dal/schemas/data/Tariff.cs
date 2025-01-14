using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
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
    [Comment("Стоимость одной подсказки")]
    public required decimal TipPrice { get; set; }

    [ConcurrencyCheck]
    [Comment("Количество бесплатных подсказок")]
    public int? FreeTipsCount { get; set; }

    [ConcurrencyCheck]
    [Comment("Количество оплаченных подсказок")]
    public int? PaidTipsCount { get; set; }

    [ConcurrencyCheck]
    [Comment("Общее количество подсказок")]
    public int? TotalTipsCount { get; set; }

    [ConcurrencyCheck]
    [Comment("Общая стоимость")]
    public decimal? TotalPrice { get; set; }

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
}
