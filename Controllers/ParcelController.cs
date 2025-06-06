using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using ParcelAuthAPI.Models;
using ParcelAuthAPI.Services;
using ParcelAuthAPI.Data;

namespace ParcelAuthAPI.Controllers
{
    [Authorize(Roles = "Sender")]
    [ApiController]
    [Route("api/[controller]")]
    public class ParcelController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly QRService _qrService;
        private readonly INotificationService _notificationService;

        public ParcelController(AppDbContext context, QRService qrService, INotificationService notificationService)
        {
            _context = context;
            _qrService = qrService;
            _notificationService = notificationService;
        }

        [HttpPost("create")]
        public async Task<IActionResult> CreateParcel([FromBody] ParcelDto parcelDto)
        {
            var senderId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            string userId = User.FindFirst("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier")?.Value;
            string role = User.FindFirst("http://schemas.microsoft.com/ws/2008/06/identity/claims/role")?.Value;

            Console.WriteLine("User ID: " + userId);
            Console.WriteLine("User Role: " + role);


            var parcel = new Parcel
            {
                SenderId = senderId,
                RecipientName = parcelDto.RecipientName,
                DeliveryAddress = parcelDto.DeliveryAddress,
                TrackingId = Guid.NewGuid().ToString()
            };

            _context.Parcels.Add(parcel);
            await _context.SaveChangesAsync();

            var qrCodeBytes = _qrService.Generate(parcel.TrackingId);
            var qrBase64 = Convert.ToBase64String(qrCodeBytes);

            var sender = await _context.Users.FindAsync(senderId);
            if (sender != null)
            {
                await _notificationService.SendEmailAsync(
                    sender.Email,
                    "Parcel Created",
                    $"Your parcel with ID {parcel.TrackingId} has been successfully created."
                );
            }

            return Ok(new { parcel.TrackingId, qrCode = $"data:image/png;base64,{qrBase64}" });
        }
    }
}
