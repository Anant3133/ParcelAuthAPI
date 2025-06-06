using System.ComponentModel.DataAnnotations;
using System;
namespace ParcelAuthAPI.Models
{
    public class NotificationLog
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public string RecipientEmail { get; set; } = string.Empty;
        public string Subject { get; set; } = string.Empty;
        public string NotificationType { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public DateTime SentAt { get; set; } = DateTime.UtcNow;
    }
}
