public class EmailConfiguration
{
    public string SenderName { get; set; } = string.Empty;
    public string SenderEmail { get; set; } = string.Empty;
    public string SenderPassword { get; set; } = string.Empty;
    public string SmtpHost { get; set; } = string.Empty;
    public int SmtpPort { get; set; }
}

public record sendEmailData(
    string toEmail,
    string subject,
    string body,
    string? attachmentPath = null);
