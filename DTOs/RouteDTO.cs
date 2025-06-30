using System.Collections.Generic;

namespace ParcelAuthAPI.DTOs
{
    public class RouteDTO
    {
        public string TrackingId { get; set; } = string.Empty;
        public string SenderAddress { get; set; } = string.Empty;
        public string DeliveryAddress { get; set; } = string.Empty;
        public List<double[]> Coordinates { get; set; } = new List<double[]>();
    }
}