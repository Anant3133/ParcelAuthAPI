namespace ParcelAuthAPI.Services
{
    public interface INotificationService
    {
        Task SendEmailAsync(string toEmail, string subject, string body);
    }
}
