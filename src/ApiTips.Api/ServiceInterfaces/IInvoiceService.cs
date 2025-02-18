using ApiTips.Dal;
using ApiTips.Dal.Enums;

namespace ApiTips.Api.ServiceInterfaces;

public interface IInvoiceService
{
    /// <summary>
    ///     Изменение статуса у счета
    /// </summary>
    /// <param name="invoice">Ссылка на счет</param>
    /// <param name="context">Контекст базы данных (нужен для отслеживания изменений)</param>
    /// <param name="newStatus">Новый статус для счета</param>
    /// <returns>True - если удалось изменить счет. False - не удалось</returns>
    bool UpdateInvoiceStatus(Dal.schemas.data.Invoice invoice, ApplicationContext context, InvoiceStatusEnum newStatus);
}