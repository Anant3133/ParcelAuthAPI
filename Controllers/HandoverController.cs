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
using System.Collections.Generic;

namespace ParcelAuthAPI.Controllers
{
    [Authorize(Roles = "Handler")]
    [ApiController]
    [Route("api/[controller]")]
    public class HandoverController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly INotificationService _notificationService;

        private readonly HashSet<string> _validStatuses = new()
        {
            "Received", "Packed", "Shipped", "Out for Delivery", "Delivered"
        };

        public HandoverController(AppDbContext context, INotificationService notificationService)
        {
            _context = context;
            _notificationService = notificationService;
        }

        [HttpPost("log")]
        public async Task<IActionResult> LogHandover([FromBody] HandoverDto dto)
        {
            if (dto == null)
                return BadRequest("Request body cannot be empty.");

            if (string.IsNullOrWhiteSpace(dto.ParcelTrackingId))
                return BadRequest("ParcelTrackingId is required.");

            if (!_validStatuses.Contains(dto.Action))
                return BadRequest($"Invalid status action. Must be one of: {string.Join(", ", _validStatuses)}.");

            if (string.IsNullOrWhiteSpace(dto.Location))
                return BadRequest("Location is required.");

            var handlerId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(handlerId))
                return Unauthorized("Handler identity not found.");

            var parcel = await _context.Parcels.FirstOrDefaultAsync(p => p.TrackingId == dto.ParcelTrackingId);
            if (parcel == null)
                return NotFound($"Parcel with TrackingId '{dto.ParcelTrackingId}' not found.");

            // Check valid status progression (optional enhancement)
            // (You can implement strict state machine here if desired)

            var handover = new Handover
            {
                ParcelTrackingId = dto.ParcelTrackingId,
                HandlerId = handlerId,
                Action = dto.Action,
                Location = dto.Location,
                Timestamp = DateTime.UtcNow
            };
            _context.Handovers.Add(handover);

            // Update parcel status and location
            parcel.Status = dto.Action;
            parcel.CurrentLocation = dto.Location;

            // Log status change
            var statusLog = new ParcelStatusLog
            {
                ParcelTrackingId = dto.ParcelTrackingId,
                Status = dto.Action,
                Timestamp = DateTime.UtcNow
            };
            _context.ParcelStatusLogs.Add(statusLog);

            // Notify sender if delivered
            if (dto.Action == "Delivered")
            {
                var sender = await _context.Users.FindAsync(parcel.SenderId);
                if (sender != null && !string.IsNullOrEmpty(sender.Email))
                {
                    await _notificationService.SendEmailAsync(
                        sender.Email,
                        "Parcel Delivered",
                        $"Your parcel {dto.ParcelTrackingId} has been successfully delivered."
                    );
                }
            }

            await _context.SaveChangesAsync();

            return Ok(new
            {
                message = $"Status '{dto.Action}' logged successfully for parcel {dto.ParcelTrackingId}."
            });
        }

        [HttpGet("handled")]
        public async Task<IActionResult> GetHandledParcels()
        {
            var handlerId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(handlerId))
                return Unauthorized("Handler identity not found.");

            var handledParcelIds = await _context.Handovers
                .Where(h => h.HandlerId == handlerId)
                .Select(h => h.ParcelTrackingId)
                .Distinct()
                .ToListAsync();

            var parcels = await _context.Parcels
                .Where(p => handledParcelIds.Contains(p.TrackingId))
                .Select(p => new
                {
                    p.TrackingId,
                    p.RecipientName,
                    p.DeliveryAddress,
                    p.Status,
                    p.CurrentLocation
                })
                .ToListAsync();

            return Ok(parcels);
        }
    }
}