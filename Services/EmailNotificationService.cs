using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;

namespace ParcelAuthAPI.Services
{
    public class EmailNotificationService : INotificationService
    {
        private readonly IConfiguration _config;
        private readonly NotificationLogger _logger;

        public EmailNotificationService(IConfiguration config, NotificationLogger logger)
        {
            _config = config;
            _logger = logger;
        }

        public async Task SendEmailAsync(string toEmail, string subject, string body)
        {
            try
            {
                var smtp = new SmtpClient
                {
                    Host = _config["Email:Host"],
                    Port = int.Parse(_config["Email:Port"] ?? "587"),
                    EnableSsl = true,
                    Credentials = new NetworkCredential(
                        _config["Email:Username"],
                        _config["Email:Password"]
                    )
                };

                var message = new MailMessage
                {
                    From = new MailAddress(_config["Email:From"]),
                    Subject = subject,
                    Body = body,
                    IsBodyHtml = false
                };

                message.To.Add(toEmail);

                await smtp.SendMailAsync(message);

                await _logger.LogNotificationAsync(toEmail, "Email", $"Subject: {subject}; Body: {body}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[Email Error] Failed to send email to {toEmail}: {ex.Message}");
                await _logger.LogNotificationAsync(toEmail, "Email Failed", ex.Message);
                // Note: Don't re-throw the error, to avoid crashing parcel creation or alert raising
            }
        }
    }
}
