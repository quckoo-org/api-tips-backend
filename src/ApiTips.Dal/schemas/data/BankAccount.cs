using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using ApiTips.Dal.Enums;
using Microsoft.EntityFrameworkCore;

namespace ApiTips.Dal.schemas.data;

public class BankAccount
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    [ConcurrencyCheck]
    [Comment("Уникальный идентификатор банковского счёта")]
    public long Id { get; set; }
    
    [Required]
    [MinLength(1)]
    [MaxLength(1000)]
    [ConcurrencyCheck]
    [Comment("Наименование банка")]
    public required string BankName { get; set;}
    
    [Required]
    [MinLength(1)]
    [MaxLength(1000)]
    [ConcurrencyCheck]
    [Comment("Адрес банка")]
    public required string BankAddress { get; set;}
    
    [Required]
    [MinLength(8)]
    [MaxLength(11)]
    [ConcurrencyCheck]
    [Comment("Swift номер")]
    public required string Swift { get; set;}
    
    [Required]
    [MinLength(8)]
    [MaxLength(11)]
    [ConcurrencyCheck]
    [Comment("Номер аккаунта")]
    public required string AccountNumber { get; set;}
    
    [Required]
    [Length(34, 34)]
    [ConcurrencyCheck]
    [Comment("Iban номер")]
    public required string Iban { get; set;}
    
    [MinLength(1)]
    [MaxLength(255)]
    [ConcurrencyCheck]
    [Comment("Дополнительная информация")]
    public string? AdditionalInfo { get; set;}


    [ConcurrencyCheck]
    [Comment("Признак запрета счёта")]
    public bool IsBaned { get; set; } = false;
    
    [ConcurrencyCheck]
    [Comment("Способ оплаты")]
    public PaymentType PaymentType { get; set; }
    
    [MinLength(1)]
    [MaxLength(255)]
    [ConcurrencyCheck]
    [Comment("Номер расчетного счета")]
    public string? PayAddress { get; set;}
    
    [ConcurrencyCheck]
    [Comment("Справочник крипто-валюты")]
    public CryptoWallet? Wallet { get; set; }
    
    [MinLength(1)]
    [MaxLength(255)]
    [ConcurrencyCheck]
    [Comment("Токен ERC, trc")]
    public string? Token { get; set;}
    
    [ConcurrencyCheck]
    [Comment("Тип валюты")]
    public CurrencyType CurrencyType { get; set; }
}