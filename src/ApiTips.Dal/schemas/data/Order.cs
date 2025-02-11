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
    [Comment("Дата создания заказа")]
    public DateTime CreateDateTime { get; set; }

    [ConcurrencyCheck]
    [Required]
    [Comment("Статус заказа")]
    public required OrderStatus Status { get; set; }

    [ConcurrencyCheck]
    [Comment("Дата оплаты заказа")]
    public DateTime? PaymentDateTime { get; set; }

    [ConcurrencyCheck]
    [Comment("Токен для получения подсказок")]
    public Guid? AccessToken { get; set; }

    [ConcurrencyCheck]
    [Required]
    [Comment("Тариф на который оформлен заказ")]
    public required Tariff Tariff { get; set; }

    [ConcurrencyCheck]
    [Required]
    [Comment("Пользователь оформивший заказ")]
    public required User User { get; set; }

    [ForeignKey("InvoiceId")]
    [Comment("Счет на который выставлен заказ")]
    public Invoice? Invoice { get; set; }
}
