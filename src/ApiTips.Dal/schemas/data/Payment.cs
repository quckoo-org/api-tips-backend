using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using ApiTips.Dal.Enums;
using ApiTips.Dal.schemas.system;
using Microsoft.EntityFrameworkCore;

namespace ApiTips.Dal.schemas.data;

[Table(nameof(Payment), Schema = "data")]
[Comment("Платежные данные")]
public class Payment
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    [ConcurrencyCheck]
    [Comment("Уникальный идентификатор банковского счёта")]
    public long Id { get; set; }
    
    [ConcurrencyCheck]
    [ForeignKey("UserId")]
    [Comment("Пользователь, которому принадлежат реквизиты")]
    public required User User { get; set; }
    
    [ConcurrencyCheck] 
    [Column(TypeName = "jsonb")]
    public PaymentDetails? Details { get; set; }
    
    /// <summary>
    ///     JsonB-структура. Платёжные данные
    /// </summary>
    public class PaymentDetails
    {
        [ConcurrencyCheck]
        public BankAccount? BankAccountDetails { get; set; }
        
        [ConcurrencyCheck] 
        public CryptoWallet? CryptoWalletDetails { get; set; }
        
        [ConcurrencyCheck]
        [Comment("Выбранный способ оплаты")]
        public PaymentType PaymentType { get; set; }
        public class CryptoWallet
        {
            [ConcurrencyCheck]
            [Comment("Номер расчётного счета")]
            public required string Аddress { get; set; }
            
            [ConcurrencyCheck]
            [Comment("Справочник крипто-валюты")]
            public required string Wallet { get; set; }
            
            [ConcurrencyCheck]
            [Comment("Токен для крипто-кошелька")]
            public required string Token { get; set; }
            
            [ConcurrencyCheck]
            [Comment("Тип крипто-валюты")]
            public CryptoType Type { get; set; }
        }
        public class BankAccount
        {
            [ConcurrencyCheck]
            [Comment("Наименование банка")]
            public required string BankName { get; set;}
    
            [ConcurrencyCheck]
            [Comment("Адрес банка")]
            public required string BankAddress { get; set;}
    
            [ConcurrencyCheck]
            [Comment("Swift номер")]
            public required string Swift { get; set;}
    
            [ConcurrencyCheck]
            [Comment("Номер аккаунта")]
            public required string AccountNumber { get; set;}
    
            [ConcurrencyCheck]
            [Comment("Iban номер")]
            public required string Iban { get; set;}
    
            [ConcurrencyCheck]
            [Comment("Дополнительная информация")]
            public string? AdditionalInfo { get; set;}


            [ConcurrencyCheck]
            [Comment("Признак запрета счёта")]
            public bool IsBaned { get; set; } = false;
            
            [ConcurrencyCheck]
            [Comment("Валюта счёта")]
            public CurrencyType Type { get; set; }
        }
    }
}