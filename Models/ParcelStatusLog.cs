using System.ComponentModel.DataAnnotations;

namespace ParcelAuthAPI.Models
{
    public class ParcelStatusLog
    {
        [Key]
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public string ParcelTrackingId { get; set; } = string.Empty;
        public string Status { get; set; } = "Packed"; 
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    }
}