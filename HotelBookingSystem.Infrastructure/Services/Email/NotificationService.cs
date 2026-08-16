using HotelBookingSystem.Application.Interfaces.IServices;
using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Configuration;
using MimeKit;

namespace HotelBookingSystem.Infrastructure.Services.Email;

public class NotificationService : INotificationService
{
    private readonly IConfiguration _configuration;

    public NotificationService(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public async Task SendAsync(string recipient, string subject, string message)
    {
        var email = new MimeMessage();

        email.From.Add(MailboxAddress.Parse(_configuration["EmailSettings:From"]!));
        email.To.Add(MailboxAddress.Parse(recipient));
        email.Subject = subject;
        email.Body = new TextPart("plain") { Text = message };

        using var smtp = new SmtpClient();

        await smtp.ConnectAsync(
            _configuration["EmailSettings:Host"]!,
            int.Parse(_configuration["EmailSettings:Port"]!),
            SecureSocketOptions.StartTls);

        await smtp.AuthenticateAsync(
            _configuration["EmailSettings:Username"]!,
            _configuration["EmailSettings:Password"]!);

        await smtp.SendAsync(email);
        await smtp.DisconnectAsync(true);
    }
}
