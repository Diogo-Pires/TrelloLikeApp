namespace EventProcessing.Mail.Interfaces;

public interface IEmailService
{
    System.Threading.Tasks.Task SendEmailAsync(string toEmail, string subject, string body, CancellationToken cancellationToken);
}
