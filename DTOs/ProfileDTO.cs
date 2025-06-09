namespace ParcelAuthAPI.DTOs
{
    public class ProfileDTO
    {
        public string Email { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
        public int ParcelCount { get; set; }
        public int HandoverCount { get; set; } 
    }
}
