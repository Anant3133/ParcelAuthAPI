using System;

namespace ParcelAuthAPI.DTOs
{
    public class ParcelStatusDTO
    {
        public string TrackingId { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public DateTime UpdatedAt { get; set; }
        public string UpdatedBy { get; set; } = string.Empty;
    }
}
