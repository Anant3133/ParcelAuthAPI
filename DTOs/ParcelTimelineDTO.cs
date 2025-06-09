using System;

namespace ParcelAuthAPI.DTOs
.DTOs
{
    public class ParcelTimelineDTO
    {
        public string TrackingId { get; set; } = string.Empty;
        public string Action { get; set; } = string.Empty;
        public string Location { get; set; } = string.Empty;
        public DateTime Timestamp { get; set; }
        public string HandledBy { get; set; } = string.Empty;       
    }
}
