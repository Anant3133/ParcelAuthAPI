using System.ComponentModel.DataAnnotations;

namespace ParcelAuthAPI.Models
{
    public class Parcel
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public string TrackingId { get; set; } = string.Empty;
        public string SenderId { get; set; } = string.Empty;
        public string SenderEmail { get; set; } = string.Empty;
        public string RecipientName { get; set; } = string.Empty;
        public string ReceiverEmail { get; set; } = string.Empty;
        public string DeliveryAddress { get; set; } = string.Empty;
        public string Status { get; set; } = "Received";
        public string CurrentLocation { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}