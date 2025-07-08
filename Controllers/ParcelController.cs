using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using ParcelAuthAPI.Models;
using ParcelAuthAPI.Services;
using ParcelAuthAPI.Data;
using System.Linq;
using System.Threading.Tasks;
using System;

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

            if (role != "Sender")
                return Forbid("User is not authorized as Sender");

            var parcel = new Parcel
            {
                SenderId = senderId,
                RecipientName = parcelDto.RecipientName,
                DeliveryAddress = parcelDto.DeliveryAddress,
                TrackingId = Guid.NewGuid().ToString(),
                SenderAddress = parcelDto.SenderAddress,      // new
                ParcelCategory = parcelDto.ParcelCategory,    // new
                Weight = parcelDto.Weight,                      // new
                Status = "Received",  // Default initial status
                CreatedAt = DateTime.UtcNow
            };

            _context.Parcels.Add(parcel);
            await _context.SaveChangesAsync();

            var qrPayload = System.Text.Json.JsonSerializer.Serialize(new
            {
                trackingId = parcel.TrackingId,
                deliveryLocation = parcel.DeliveryAddress 
            });

            var qrCodeBytes = _qrService.Generate(qrPayload);
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

            if (role != "Sender")
                return Forbid("User is not authorized as Sender");

            var parcels = _context.Parcels
                .Where(p => p.SenderId == senderId)
                .Select(p => new
                {
                    p.TrackingId,
                    p.RecipientName,
                    p.DeliveryAddress,
                    p.Status,
                    p.SenderAddress,       // new
                    p.ParcelCategory,      // new
                    p.Weight,              // new
                    p.CurrentLocation,
                    p.CreatedAt
                })
                .ToList();

            return Ok(parcels);
        }

        [HttpGet("qrcode/{trackingId}")]
        public IActionResult GetParcelQrCode(string trackingId)
        {
            var parcel = _context.Parcels.FirstOrDefault(p => p.TrackingId == trackingId);
            if (parcel == null) return NotFound("Parcel not found");

            var qrPayload = System.Text.Json.JsonSerializer.Serialize(new
            {
                trackingId = parcel.TrackingId,
                deliveryLocation = parcel.DeliveryAddress
            });

            var qrCodeBytes = _qrService.Generate(qrPayload);
            var qrBase64 = Convert.ToBase64String(qrCodeBytes);

            return Ok(new { qrCode = $"data:image/png;base64,{qrBase64}" });
        }

        [HttpGet("handled")]
        [Authorize(Roles = "Handler")]
        public IActionResult GetHandledParcels()
        {
            var handlerId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var role = User.FindFirst(ClaimTypes.Role)?.Value;

            if (role != "Handler")
                return Forbid("User is not authorized as Handler");

            var handledTrackingIds = _context.Handovers
                .Where(h => h.HandlerId == handlerId)
                .Select(h => h.ParcelTrackingId)
                .Distinct()
                .ToList();

            var parcels = _context.Parcels
                .Where(p => handledTrackingIds.Contains(p.TrackingId))
                .Select(p => new
                {
                    p.TrackingId,
                    p.RecipientName,
                    p.DeliveryAddress,
                    p.SenderAddress,       // new
                    p.ParcelCategory,      // new
                    p.Weight,              // new
                    p.Status,
                    p.CreatedAt,
                    p.CurrentLocation
                })
                .ToList();

            return Ok(parcels);
        }
    }
}