namespace ParcelAuthAPI.Models
{
    public class Handover
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public string ParcelTrackingId { get; set; } = string.Empty;  
        public string HandlerId { get; set; } = string.Empty; 
        public string Action { get; set; } = string.Empty;            
        public string Location { get; set; } = string.Empty;
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
        public DateTime HandoverTime { get; set; } = DateTime.UtcNow;
    }
}
