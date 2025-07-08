namespace ParcelAuthAPI.DTOs
{
    public class UserUpdateDto
    {
        public string Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public DateTime Birthday { get; set; } 
        public string Mobile { get; set; } = string.Empty;
        public string Location { get; set; } = string.Empty;
        public string Languages { get; set; } = string.Empty;
        public string About { get; set; } = string.Empty;
    }
}