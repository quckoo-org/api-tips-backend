namespace ApiTips.Api.ServiceInterfaces;

public interface IEmail
{
    Task SendEmailAsync(string to, string subject, string body);
}