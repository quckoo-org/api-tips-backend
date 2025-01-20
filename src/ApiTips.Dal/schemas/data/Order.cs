using ApiTips.Dal.schemas.system;
using ApiTips.Dal.Enums;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ApiTips.Dal.schemas.data;

[Table(nameof(Order), Schema = "data")]
[Comment("Объект - заказ")]
public class Order
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    [ConcurrencyCheck]
    [Comment("Уникальный идентификатор заказа")]
    public long Id { get; set; }

    [ConcurrencyCheck]
    [Required]
    [Comment("Дата создания заказа")]
    public required DateTime CreateDateTime { get; set; }

    [ConcurrencyCheck]
    [Required]
    [Comment("Статус заказа")]
    public required OrderStatus Status { get; set; }

    [ConcurrencyCheck]
    [Comment("Дата оплаты заказа")]
    public DateTime? PaymentDateTime { get; set; }

    [ConcurrencyCheck]
    [Required]
    [Comment("Тариф на который оформлен заказ")]
    public required Tariff Tariff { get; set; }

    [ConcurrencyCheck]
    [Required]
    [Comment("Пользователь оформивший заказ")]
    public required User User { get; set; }

    // to do Ссылка на счёт
}
