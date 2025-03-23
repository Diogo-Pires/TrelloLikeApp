namespace EventProcessing.Mail.Settings;

public record SmtpSettings
{
    public required string Host { get; set; }
    public required int Port { get; set; }
    public required string Username { get; set; }
    public required string Email { get; set; }
    public required string Password { get; set; }
    public required bool UseSsl { get; set; }
}