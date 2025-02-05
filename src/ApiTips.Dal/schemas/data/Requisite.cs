using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using ApiTips.Dal.Enums;
using ApiTips.Dal.schemas.system;
using Microsoft.EntityFrameworkCore;

namespace ApiTips.Dal.schemas.data;

public class Requisite
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    [ConcurrencyCheck]
    [Comment("Уникальный идентификатор заказа")]
    public long Id { get; set; }

    [ConcurrencyCheck]
    [Comment("Признак запрета счета")]
    public bool IsBanned { get; set; }

    [ConcurrencyCheck]
    [Required]
    [Comment("Статус заказа")]
    public required PaymentType PaymentType { get; set; }



    [ConcurrencyCheck]
    [Required]
    [Column(TypeName = "jsonb")]
    [Comment("Реквизиты оплаты")]
    public required PaymentDetails PaymentRequisites { get; set; }

    /// <summary>
    ///     JsonB-структура. Платёжные данные
    /// </summary>
    public class PaymentDetails
    {
        [ConcurrencyCheck]
        public BankAccount? BankAccountDetails { get; set; }
        
        [ConcurrencyCheck] 
        public CryptoWallet? CryptoWalletDetails { get; set; }
        
        public class CryptoWallet
        {
            [ConcurrencyCheck]
            [Comment("Номер расчётного счета")]
            public required string Address { get; set; }
            
            [ConcurrencyCheck]
            [Comment("Справочник крипто-валюты")]
            public required string Wallet { get; set; }
            
            [ConcurrencyCheck]
            [Comment("Токен для крипто-кошелька")]
            public required string Token { get; set; }
            
            [ConcurrencyCheck]
            [Comment("Тип крипто-валюты")]
            public required string Type { get; set; }
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
            [Comment("Валюта счёта")]
            public string Type { get; set; }
        }
    }

}