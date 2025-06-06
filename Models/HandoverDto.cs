namespace ParcelAuthAPI.Models
{
    public class HandoverDto
    {
        public string ParcelTrackingId { get; set; } = string.Empty;
        public string Action { get; set; } = string.Empty;
        public string Location { get; set; } = string.Empty;
    }
}
