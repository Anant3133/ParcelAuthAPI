using ParcelAuthAPI.Data;
using ParcelAuthAPI.Models;
using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;

namespace ParcelAuthAPI.Services
{
    public class NotificationLogger
    {
        private readonly AppDbContext _context;
        private readonly IConfiguration _config;

        public NotificationLogger(AppDbContext context, IConfiguration config)
        {
            _context = context;
            _config = config;
        }

        public async Task LogNotificationAsync(string recipientEmail, string notificationType, string message)
        {
            var log = new NotificationLog
            {
                RecipientEmail = recipientEmail,
                NotificationType = notificationType,
                Message = message,
                SentAt = DateTime.UtcNow
            };

            _context.NotificationLogs.Add(log);
            await _context.SaveChangesAsync();
        }
    }
}