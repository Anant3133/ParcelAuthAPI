using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using ParcelAuthAPI.Models;
using ParcelAuthAPI.Services;
using ParcelAuthAPI.Data;
using System.Linq;

namespace ParcelAuthAPI.Controllers
{
    [Authorize]
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
            var role = User.FindFirst(ClaimTypes.Role)?.Value;

            Console.WriteLine("=== CreateParcel ===");
            Console.WriteLine("Sender ID: " + senderId);
            Console.WriteLine("User Role: " + role);

            if (role != "Sender")
                return Forbid("User is not authorized as Sender");

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

        [HttpGet("my")]
        public IActionResult GetMyParcels()
        {
            var senderId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var role = User.FindFirst(ClaimTypes.Role)?.Value;

            Console.WriteLine("=== GetMyParcels ===");
            Console.WriteLine("Sender ID: " + senderId);
            Console.WriteLine("User Role: " + role);

            if (role != "Sender")
                return Forbid("User is not authorized as Sender");

            var parcels = _context.Parcels
                .Where(p => p.SenderId == senderId)
                .Select(p => new
                {
                    p.TrackingId,
                    p.RecipientName,
                    p.DeliveryAddress,
                    p.Status
                })
                .ToList();

            return Ok(parcels);
        }

        [HttpGet("handled")]
        public IActionResult GetHandledParcels()
        {
            var handlerId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var role = User.FindFirst(ClaimTypes.Role)?.Value;

            Console.WriteLine("=== GetHandledParcels ===");
            Console.WriteLine("Handler ID: " + handlerId);
            Console.WriteLine("User Role: " + role);

            if (role != "Handler")
                return Forbid("User is not authorized as Handler");

            // Fix: Replace 'TrackingId' with 'ParcelTrackingId' as per the Handover class definition
            var handledTrackingIds = _context.Handovers
                .Where(h => h.HandlerId == handlerId)
                .Select(h => h.ParcelTrackingId) // Correct property name
                .Distinct()
                .ToList();

            var parcels = _context.Parcels
                .Where(p => handledTrackingIds.Contains(p.TrackingId))
                .Select(p => new
                {
                    p.TrackingId,
                    p.RecipientName,
                    p.DeliveryAddress,
                    p.Status
                })
                .ToList();

            return Ok(parcels);
        }
    }
}