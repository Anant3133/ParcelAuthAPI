using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using ParcelAuthAPI.Models;
using ParcelAuthAPI.Data;
using ParcelAuthAPI.Services;
using System.Linq;
using System.Threading.Tasks;
using System;

namespace ParcelAuthAPI.Controllers
{
    [Authorize(Roles = "Handler")]
    [ApiController]
    [Route("api/[controller]")]
    public class HandoverController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly INotificationService _notificationService;

        public HandoverController(AppDbContext context, INotificationService notificationService)
        {
            _context = context;
            _notificationService = notificationService;
        }

        [HttpPost("log")]
        public async Task<IActionResult> LogHandover([FromBody] HandoverDto dto)
        {
            if (dto.Action != "Received" && dto.Action != "HandedOver")
                return BadRequest("Invalid action. Must be 'Received' or 'HandedOver'.");

            var handlerId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var lastAction = await _context.Handovers
                .Where(h => h.ParcelTrackingId == dto.ParcelTrackingId)
                .OrderByDescending(h => h.Timestamp)
                .FirstOrDefaultAsync();

            bool isTampered = false;
            string tamperReason = "";

            if (lastAction != null)
            {
                if (lastAction.Action == dto.Action)
                {
                    isTampered = true;
                    tamperReason = $"Repeated '{dto.Action}' without alternating.";
                }
            }

            var handover = new Handover
            {
                ParcelTrackingId = dto.ParcelTrackingId,
                HandlerId = handlerId,
                Action = dto.Action,
                Location = dto.Location,
                Timestamp = DateTime.UtcNow
            };

            _context.Handovers.Add(handover);

            if (isTampered)
            {
                var alert = new TamperAlert
                {
                    ParcelTrackingId = dto.ParcelTrackingId,
                    HandlerId = handlerId,
                    Message = tamperReason,
                    Timestamp = DateTime.UtcNow
                };
                _context.TamperAlerts.Add(alert);

                var admins = await _context.Users.Where(u => u.Role == "Admin").ToListAsync();
                foreach (var admin in admins)
                {
                    await _notificationService.SendEmailAsync(
                        admin.Email,
                        "Tampering Detected",
                        $"Tamper alert for parcel {dto.ParcelTrackingId}: {tamperReason}"
                    );
                }
            }

            if (dto.Action == "HandedOver")
            {
                var parcel = await _context.Parcels.FirstOrDefaultAsync(p => p.TrackingId == dto.ParcelTrackingId);
                var sender = await _context.Users.FindAsync(parcel?.SenderId);
                if (sender != null)
                {
                    await _notificationService.SendEmailAsync(
                        sender.Email,
                        "Parcel Delivered",
                        $"Your parcel {dto.ParcelTrackingId} has been marked as delivered."
                    );
                }
            }

            await _context.SaveChangesAsync();

            return Ok(new
            {
                message = "Handover logged successfully",
                tampered = isTampered,
                tamperNote = isTampered ? tamperReason : null
            });
        }

        // New endpoint to fetch parcels handled by current handler
        [HttpGet("handled")]
        public IActionResult GetHandledParcels()
        {
            var handlerId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            // Get distinct parcel IDs this handler has handled
            var handledParcelIds = _context.Handovers
                .Where(h => h.HandlerId == handlerId)
                .Select(h => h.ParcelTrackingId)
                .Distinct()
                .ToList();

            // Fetch parcels details for those IDs
            var parcels = _context.Parcels
                .Where(p => handledParcelIds.Contains(p.TrackingId))
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