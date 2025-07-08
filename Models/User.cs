namespace ParcelAuthAPI.Models;

public class User
{
    public string Id { get; set; } = Guid.NewGuid().ToString(); 
    public string Email { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public string Role { get; set; } = "Sender";
    public string Name { get; set; } = string.Empty;
    public string? ProfilePictureUrl { get; set; }
    public string? BannerImageUrl { get; set; }
    public DateTime Birthday { get; set; }  
    public string Mobile { get; set; } = string.Empty;
    public string Location { get; set; } = string.Empty;
    public string Languages { get; set; } = string.Empty;
    public string About { get; set; } = string.Empty;
}