using System.Threading.Channels;
using ECommerce.Util;

namespace ECommerce.Service;

public class EmailBackgroundService : BackgroundService
{
    private readonly Channel<sendEmailData> emailChannel = Channel.CreateUnbounded<sendEmailData>();
    private readonly IEmailSender emailSender;

    public EmailBackgroundService(
        IEmailSender emailSender)
    {
        this.emailSender = emailSender;
    }

    public async Task QueueEmail(sendEmailData emailData)
    {
        await this.emailChannel.Writer.WriteAsync(emailData);
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                var emailData = await this.emailChannel.Reader.ReadAsync(stoppingToken);
                await this.emailSender.SendEmail(stoppingToken, emailData);
            }
            catch (OperationCanceledException)
            {
                await this.emailSender.Shutdown(default);
                break;
            }
        }
    }
}

