namespace ParcelAuthAPI.Models
{
    public class TamperAlert
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public string ParcelTrackingId { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public string HandlerId { get; set; } = string.Empty;
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    }
}
