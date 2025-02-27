using ApiTips.Api.MapperProfiles.Method;
using ApiTips.Api.MapperProfiles.Permission;
using ApiTips.Api.MapperProfiles.Role;
using ApiTips.Api.MapperProfiles.User;
using ApiTips.Api.ServiceInterfaces;
using ApiTips.Dal;
using ApiTips.Dal.Enums;
using AutoMapper;
using Microsoft.EntityFrameworkCore;

namespace ApiTips.Api.Services.Common;

public class InvoiceService(ILogger<InvoiceService> logger) : IInvoiceService
{

    public bool UpdateInvoiceStatus(Dal.schemas.data.Invoice invoice,
        ApplicationContext context, InvoiceStatusEnum newStatus)
    {
        switch (newStatus)
        {
            case InvoiceStatusEnum.Cancelled:
                return SetInvoiceStatusCanceled(invoice, context);
            case InvoiceStatusEnum.Paid:
                return SetInvoiceStatusPaid(invoice, context);
            case InvoiceStatusEnum.Created:
                return SetInvoiceStatusCreated(invoice, context);
            default:
                return false;
        }
    }

    /// <summary>
    ///     Установка счету статуса "Оплачен"
    /// </summary>
    /// <param name="invoice">Ссылка на счет</param>
    /// <param name="paymentDate">Дата оплаты счета</param>
    /// <param name="context">Контекст базы данных (нужен для отслеживания изменений)</param>
    /// <returns>True - если удалось изменить счет. False - не удалось</returns>
    private bool SetInvoiceStatusPaid(Dal.schemas.data.Invoice invoice, ApplicationContext context)
    {
        if (invoice.Status == InvoiceStatusEnum.Cancelled || invoice.Order?.Status == OrderStatus.Cancelled)
        {
            logger.LogError("Нельзя выставить статус счёта \"Оплачен\": Guid счета - {InvoiceId}, Id заказа - {OrderId}"
                , invoice.Id, invoice.Order.Id);
            return false;
        }
        
        invoice.Status = InvoiceStatusEnum.Paid;

        return context.Entry(invoice).State == EntityState.Modified;
    }

    /// <summary>
    ///     Метод устанавливает счету статус "Отменен"
    /// </summary>
    /// <param name="invoice">Ссылка на счёт</param>
    /// <param name="context">Контекст базы данных</param>
    /// <returns></returns>
    private bool SetInvoiceStatusCanceled(Dal.schemas.data.Invoice invoice, ApplicationContext context)
    {
        // Установка счету статуса "Отменен". 
        invoice.Status = InvoiceStatusEnum.Cancelled;
        invoice.PayedAt = null;

        return context.Entry(invoice).State == EntityState.Modified;
    }

    /// <summary>
    ///     Установка счету статус "Создан"
    /// </summary>
    /// <param name="invoice"></param>
    /// <param name="context"></param>
    /// <returns></returns>
    private bool SetInvoiceStatusCreated(Dal.schemas.data.Invoice invoice, ApplicationContext context)
    {
        if (invoice.Status == InvoiceStatusEnum.Paid)
        {
            logger.LogError("Нельзя выставить статус счёта \"Создан\", счет уже оплачен: Guid счета - {InvoiceId}"
                , invoice.Id);
            return false;
        }
        
        invoice.Status = InvoiceStatusEnum.Created;

        return context.Entry(invoice).State == EntityState.Modified;
    }
}