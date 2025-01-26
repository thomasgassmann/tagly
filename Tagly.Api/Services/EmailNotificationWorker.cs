using System.Net;
using System.Net.Mail;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Tagly.Db;

namespace Tagly.Api.Services;

public class EmailNotificationWorker(ILogger<EmailNotificationWorker> logger, IServiceScopeFactory scopeFactory, IConfiguration config) : BackgroundService
{
    private readonly HashSet<long> _seenIds = [];
    
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            await CheckItemsAsync();
            
            await Task.Delay(Convert.ToInt32(config["CheckInterval"]), stoppingToken);
        }
    }

    private async Task CheckItemsAsync()
    {
        if (!Convert.ToBoolean(config["EnableNotifications"]))
        {
            return;
        }
        
        using var scope = scopeFactory.CreateScope();
        var context = scope.ServiceProvider.GetService<TaglyContext>()!;
        var currentPhotos = await context.Photos.AsNoTracking().ToListAsync();
        var newPhotos = currentPhotos.Where(x => !_seenIds.Contains(x.Id)).ToList();
        if (newPhotos.Any())
        {
            await SendStatusEmailAsync(newPhotos);
            foreach (var newPhoto in newPhotos)
            {
                _seenIds.Add(newPhoto.Id);
            }
        }
    }

    private async Task SendStatusEmailAsync(IList<StoredPhoto> newPhotos)
    {
        var client = new SmtpClient(config["Smtp:Host"]);
        client.Port = Convert.ToInt32(config["Smtp:Port"]);
        client.UseDefaultCredentials = false;
        client.EnableSsl = Convert.ToBoolean(config["Smtp:EnableSsl"]);
        client.Credentials = new NetworkCredential(config["Smtp:User"], config["Smtp:Password"]);

        var message = new MailMessage();
        message.From = new MailAddress(config["Smtp:User"]!);
        var recipients = config.GetSection("MailRecipients").Get<string[]>()!
            .Select(x => new MailAddress(x));
        foreach (var recipient in recipients)
        {
            message.To.Add(recipient);
        }
        
        message.Subject = $"Tagly - {newPhotos.Count} new photos";
        message.IsBodyHtml = false;
        var body = new StringBuilder("There are new uploaded photos:");
        body.AppendLine();
        body.AppendLine("Id / Filename / Latitude / Longitude / Date / Description");
        foreach (var photo in newPhotos)
        {
            body.AppendLine($"{photo.Id} / {photo.FileName} / {photo.Latitude} / {photo.Longitude} / {photo.Date:yyyy-MM-dd HH:mm} / {photo.Description}");
        }
        
        message.Body = body.ToString();
        try
        {
            await client.SendMailAsync(message);
        }
        catch (Exception e)
        {
            logger.LogError("Failed to send email: {}", e.Message);
        }
    }
}