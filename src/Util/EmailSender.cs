using MimeKit;
using MailKit.Net.Smtp;

namespace ECommerce.Util;

public interface IEmailSender
{
    Task SendEmail(
        CancellationToken ct,
        sendEmailData data);
    Task Shutdown(CancellationToken ct);
}

public class EtherealEmailSender : IEmailSender
{
    private readonly EmailConfiguration emailConfiguration;
    private readonly ILogger<EtherealEmailSender> logger;

    public EtherealEmailSender(
        IConfiguration configuration,
        EmailConfiguration emailConfiguration,
        ILogger<EtherealEmailSender> logger)
    {
        // this.emailConfiguration = new();
        // configuration.GetSection("EtherealEmailConfiguration").Bind(this.emailConfiguration);
        this.emailConfiguration = emailConfiguration;
        this.logger = logger;
    }

    public async Task SendEmail(
        CancellationToken ct,
        sendEmailData data)
    {
        try
        {
            var cfg = this.emailConfiguration!;
            this.logger.LogInformation($"Sender Email {cfg.SenderEmail}, Sender Name {cfg.SenderName}");

            var message = new MimeMessage();
            message.From.Add(new MailboxAddress(
                cfg.SenderEmail,
                cfg.SenderEmail));
            message.To.Add(new MailboxAddress(
                "",
                data.toEmail));
            message.Subject = $@"{data.subject}";

            var bodyBuilder = new BodyBuilder
            {
                TextBody = data.body
            };

            if (!string.IsNullOrEmpty(data.attachmentPath) && File.Exists(data.attachmentPath))
            {
                bodyBuilder.Attachments.Add(data.attachmentPath);
            }

            message.Body = bodyBuilder.ToMessageBody();

            using var smtp = new SmtpClient();
            await smtp.ConnectAsync(
                    this.emailConfiguration.SmtpHost,
                    this.emailConfiguration.SmtpPort,
                    MailKit.Security.SecureSocketOptions.StartTls);

            await smtp.AuthenticateAsync(
                    this.emailConfiguration.SenderEmail,
                    this.emailConfiguration.SenderPassword);

            var result = await smtp.SendAsync(message);
            await smtp.DisconnectAsync(true, ct);

            this.logger.LogInformation($"Sending ethereal email to {data.toEmail} success {result.ToString()}");
        }
        catch (System.Exception err)
        {
            this.logger.LogError($"Sending ethereal email to {data.toEmail} error {err.Message}");
            throw;
        }
    }

    public Task Shutdown(CancellationToken ct)
    {
        throw new Exception("Not implemented");
    }
}

public class GmailEmailSender : IEmailSender
{
    private readonly EmailConfiguration? emailConfiguration;
    private readonly SmtpClient client;
    private readonly ILogger<GmailEmailSender> logger;

    public GmailEmailSender(
        EmailConfiguration emailConfiguration,
        ILogger<GmailEmailSender> logger)
    {

        // this.emailConfiguration = new EmailConfiguration();
        // configuration.GetSection("GmailConfiguration").Bind(this.emailConfiguration);

        this.emailConfiguration = emailConfiguration;
        this.client = new SmtpClient();
        client.Connect(
                this.emailConfiguration.SmtpHost,
                this.emailConfiguration.SmtpPort,
                MailKit.Security.SecureSocketOptions.StartTls);

        client.Authenticate(
                this.emailConfiguration.SenderEmail,
                this.emailConfiguration.SenderPassword);
        this.logger = logger;
        this.emailConfiguration = emailConfiguration;
    }

    public async Task SendEmail(
        CancellationToken ct,
        sendEmailData data)
    {
        try
        {
            var cfg = this.emailConfiguration!;
            var message = new MimeMessage();
            this.logger.LogInformation($"Sender email {cfg.SenderEmail}, sender name {cfg.SenderName}");

            message.From.Add(new MailboxAddress(
                cfg.SenderEmail,
                cfg.SenderEmail));
            message.To.Add(new MailboxAddress(
                data.toEmail,
                data.toEmail));
            message.Subject = $@"{data.subject}";

            var bodyBuilder = new BodyBuilder
            {
                TextBody = data.body
            };

            if (!string.IsNullOrEmpty(data.attachmentPath) && File.Exists(data.attachmentPath))
            {
                bodyBuilder.Attachments.Add(data.attachmentPath);
            }

            message.Body = bodyBuilder.ToMessageBody();

            var result = await this.client.SendAsync(message, ct);
            await this.client.DisconnectAsync(true, ct);

            this.logger.LogInformation($"Sending gmail email to {data.toEmail} success [{result.ToString()}]");
        }
        catch (System.Exception err)
        {
            this.logger.LogError($"Sending gmail email to {data.toEmail} error" + err.Message);
            throw;
        }
    }

    public async Task Shutdown(CancellationToken ct)
    {
        await this.client.DisconnectAsync(false, ct);
    }

}
