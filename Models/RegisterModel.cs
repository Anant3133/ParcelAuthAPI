namespace ParcelAuthAPI.Models;
public class RegisterModel
{
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string Role { get; set; } = "Sender";
}