using MimeKit;
using MailKit.Net.Smtp;

namespace ECommerce.Util;

public interface IEmailSender
{
    Task SendEmail(CancellationToken ct, sendEmailData data);
    Task Shutdown(CancellationToken ct);
}

public class EtherealEmailSender : IEmailSender
{
    private readonly EmailConfiguration emailConfiguration;
    private readonly SmtpClient client;

    public EtherealEmailSender(IConfiguration configuration)
    {
        this.emailConfiguration = new();
        configuration.GetSection("EtherealEmailConfiguration").Bind(this.emailConfiguration);

        this.client = new SmtpClient();
        client.Connect(
                this.emailConfiguration.SmtpHost,
                this.emailConfiguration.SmtpPort,
                MailKit.Security.SecureSocketOptions.StartTls);

        client.Authenticate(
                this.emailConfiguration.SenderEmail,
                this.emailConfiguration.SenderPassword);
    }

    public async Task SendEmail(CancellationToken ct, sendEmailData data)
    {
        try
        {
            var cfg = this.emailConfiguration!;
            var message = new MimeMessage();
            message.From.Add(new MailboxAddress(
                cfg.SenderEmail,
                cfg.SenderEmail));
            message.To.Add(new MailboxAddress(
                data.toEmail,
                data.toEmail));
            message.Subject = $@"{data.subject}";

            message.Body = new TextPart(MimeKit.Text.TextFormat.Plain)
            {
                Text = data.body,
            };


            await this.client.SendAsync(message, ct);
            // await client.DisconnectAsync(false, ct);

            Console.WriteLine($"Sending ethereal email to {data.toEmail} success");
        }
        catch (System.Exception err)
        {
            Console.WriteLine($"Sending ethereal email to {data.toEmail} error" + err.Message);
            throw;
        }
    }

    public async Task Shutdown(CancellationToken ct)
    {
        await this.client.DisconnectAsync(false, ct);
    }
}

public class GmailEmailSender : IEmailSender
{
    private readonly EmailConfiguration? emailConfiguration;
    private readonly SmtpClient client;

    public GmailEmailSender(IConfiguration configuration)
    {

        this.emailConfiguration = new EmailConfiguration();
        configuration.GetSection("GmailConfiguration").Bind(this.emailConfiguration);

        this.client = new SmtpClient();
        client.Connect(
                this.emailConfiguration.SmtpHost,
                this.emailConfiguration.SmtpPort,
                MailKit.Security.SecureSocketOptions.StartTls);

        client.Authenticate(
                this.emailConfiguration.SenderEmail,
                this.emailConfiguration.SenderPassword);

    }

    public async Task SendEmail(CancellationToken ct, sendEmailData data)
    {
        try
        {
            var cfg = this.emailConfiguration!;
            var message = new MimeMessage();
            message.From.Add(new MailboxAddress(
                cfg.SenderEmail,
                cfg.SenderEmail));
            message.To.Add(new MailboxAddress(data.toEmail, data.toEmail));
            message.Subject = $@"{data.subject}";

            message.Body = new TextPart(MimeKit.Text.TextFormat.Plain)
            {
                Text = data.body,
            };

            await this.client.SendAsync(message, ct);

            Console.WriteLine($"Sending gmail email to {data.toEmail} success");
        }
        catch (System.Exception err)
        {
            Console.WriteLine($"Sending gmail email to {data.toEmail} error" + err.Message);
            throw;
        }
    }

    public async Task Shutdown(CancellationToken ct)
    {
        await this.client.DisconnectAsync(false, ct);
    }

}
