namespace ParcelAuthAPI.Models
{
    public class ParcelDto
    {
        public string RecipientName { get; set; } = string.Empty;
        public string DeliveryAddress { get; set; } = string.Empty;
        public string SenderAddress { get; set; } = string.Empty;
        public string ParcelCategory { get; set; } = string.Empty;  // e.g. "Fragile", "Heavy", "Standard"
        public decimal Weight { get; set; }
    }
}
