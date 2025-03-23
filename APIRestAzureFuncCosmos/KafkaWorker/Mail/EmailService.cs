using EventProcessing.Mail.Interfaces;
using EventProcessing.Mail.Settings;
using EventProcessing.Middlewares;
using MailKit.Net.Smtp;
using Microsoft.Extensions.Options;
using MimeKit;

namespace EventProcessing.Mail
{
    internal class EmailService(IOptions<SmtpSettings> smtpSettings, GlobalExceptionHandler exceptionHandler) : IEmailService
    {
        private readonly SmtpSettings _smtpSettings = smtpSettings.Value;
        private readonly GlobalExceptionHandler _exceptionHandler = exceptionHandler;

        public async System.Threading.Tasks.Task SendEmailAsync(string toEmail,
                                                                string subject,
                                                                string body,
                                                                CancellationToken cancellationToken)
        {
            var message = new MimeMessage();
            message.From.Add(new MailboxAddress(_smtpSettings.Username, _smtpSettings.Email));
            message.To.Add(new MailboxAddress(toEmail, toEmail));
            message.Subject = subject;
            message.Body = new TextPart("html") { Text = body };

            try
            {
                using var smtpClient = new SmtpClient();
                await smtpClient.ConnectAsync(_smtpSettings.Host, _smtpSettings.Port, _smtpSettings.UseSsl ? MailKit.Security.SecureSocketOptions.SslOnConnect : MailKit.Security.SecureSocketOptions.StartTls, cancellationToken);
                await smtpClient.AuthenticateAsync(_smtpSettings.Username, _smtpSettings.Password, cancellationToken);
                await smtpClient.SendAsync(message, cancellationToken);
                await smtpClient.DisconnectAsync(true, cancellationToken);
            }
            catch (Exception ex)
            {
                await _exceptionHandler.HandleExceptionAsync(ex, cancellationToken);
            }
        }
    }
}