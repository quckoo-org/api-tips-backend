using ApiTips.Api.ServiceInterfaces;
using MailKit.Net.Smtp;
using MimeKit;

namespace ApiTips.Api.Services;

public class EmailService : IEmail
{
    private readonly string _host;
    private readonly int _port;
    private readonly string _from;
    private readonly string _password;
    private readonly bool _useSsl;
    
    
    public EmailService(IConfiguration configuration)
    {
        _host = configuration.GetValue<string>("Smtp:Host") ?? string.Empty;
        _port = configuration.GetValue<int>("Smtp:Port");
        _from = configuration.GetValue<string>("Smtp:From") ?? string.Empty;
        _password = Environment.GetEnvironmentVariable("SMTP_PASSWORD") ?? string.Empty;
        _useSsl = configuration.GetValue<bool>("Smtp:UseSsl");

    }

    public async Task SendEmailAsync(string to, string subject, string body)
    {
        var email = new MimeMessage();
        email.From.Add(new MailboxAddress("Администратор quckoo.net", _from));
        email.To.Add(new MailboxAddress("", to));
        email.Subject = subject;

        var bodyBuilder = new BodyBuilder { HtmlBody = body };
        email.Body = bodyBuilder.ToMessageBody();

        using var smtp = new SmtpClient();
        await smtp.ConnectAsync(_host, _port, _useSsl);
        await smtp.AuthenticateAsync(_from, _password);
        await smtp.SendAsync(email);
        await smtp.DisconnectAsync(true);
    }
}