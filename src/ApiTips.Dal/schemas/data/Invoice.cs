using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using ApiTips.Dal.Enums;
using ApiTips.Dal.schemas.system;
using Microsoft.EntityFrameworkCore;

namespace ApiTips.Dal.schemas.data;

[Table(nameof(Invoice), Schema = "data")]
[Comment("Объект - заказ")]
public class Invoice
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    [ConcurrencyCheck]
    [Comment("Уникальный идентификатор счета")]
    public Guid Id { get; set; }
    
    [Required]
    [ConcurrencyCheck]
    [Comment("Плательщик счета")]
    public required User Payer { get; set; }
    
    [Required]
    [ConcurrencyCheck]
    [ForeignKey("OrderId")]
    [Comment("Заказ, по которому выставлен счет")]
    public required Order Order { get; set; }
    
    [Required]
    [ConcurrencyCheck]
    [Comment("REF-номер заказа, по которому выставлен счет")]
    public string RefNumber { get; set; } = string.Empty;

    [Required]
    [ConcurrencyCheck]
    [Comment("Алиас счёта, генерирующийся при создании")]
    public string Alias { get; set; } = string.Empty;
    
    [Required]
    [ConcurrencyCheck]
    [Comment("Общее количество запросов")]
    public long AmountOfRequests { get; set; }
    
    [Required]
    [ConcurrencyCheck]
    [Comment("Дата создания счета")]
    public required DateTime CreatedAt { get; set; }
    
    [ConcurrencyCheck]
    [Comment("Дата оплаты счета")]
    public DateTime? PayedAt { get; set; }
    
    [ConcurrencyCheck]
    [Comment("Комментарий к счету")]
    public string? Description { get; set; }

    
    [Required]
    [ConcurrencyCheck]
    [Comment("Валюта для оплаты счета")]
    [Column(TypeName = "jsonb")]
    public Currency CurrentCurrency { get; set; }
    
    public class Currency
    {
        [Required]
        [ConcurrencyCheck]
        [Comment("Способ оплаты")]
        public required PaymentType Type { get; set; }
        
        [ConcurrencyCheck]
        [Comment("Сумма для оплаты")]
        public required decimal TotalAmount { get; set; }
        
        [ConcurrencyCheck]
        [Comment("Валюта для оплаты")]
        public required string CurrencyType { get; set; }
    }
}