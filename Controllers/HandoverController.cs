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

       
        private readonly List<string> _statusOrder = new()
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

    // Fetch last status log for this parcel
    var lastStatusLog = await _context.ParcelStatusLogs
        .Where(s => s.ParcelTrackingId == dto.ParcelTrackingId)
        .OrderByDescending(s => s.Timestamp)
        .FirstOrDefaultAsync();

    // Define the status progression order
    var statusFlow = new List<string> { "Received", "Packed", "Shipped", "Out for Delivery", "Delivered" };

    int lastIndex = lastStatusLog != null ? statusFlow.IndexOf(lastStatusLog.Status) : -1;
    int currentIndex = statusFlow.IndexOf(dto.Action);

    // Check for skipped steps in status progression
    if (currentIndex == -1)
    {
        return BadRequest("Invalid status provided.");
    }
    if (lastIndex != -1 && currentIndex > lastIndex + 1)
    {
        // Raise tamper alert for skipped step
        var tamperAlert = new TamperAlert
        {
            ParcelTrackingId = dto.ParcelTrackingId,
            Message = $"Status skipped from '{lastStatusLog.Status}' to '{dto.Action}'. Possible tampering detected.",
            HandlerId = handlerId,
            Timestamp = DateTime.UtcNow
        };
        _context.TamperAlerts.Add(tamperAlert);
    }

    // Check for minimum time interval between status updates (e.g., 1 hour)
    if (lastStatusLog != null)
    {
        var timeDiff = DateTime.UtcNow - lastStatusLog.Timestamp;
        var minimumInterval = TimeSpan.FromHours(1); // set minimum realistic interval here
        if (timeDiff < minimumInterval)
        {
            // Raise tamper alert for too quick status update
            var tamperAlert = new TamperAlert
            {
                ParcelTrackingId = dto.ParcelTrackingId,
                Message = $"Status updated too quickly after previous update ({timeDiff.TotalMinutes:F1} minutes). Possible tampering detected.",
                HandlerId = handlerId,
                Timestamp = DateTime.UtcNow
            };
            _context.TamperAlerts.Add(tamperAlert);
        }
    }

    // Log the handover
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

        [HttpGet("parcels")]
        [Authorize(Roles = "Admin , Handler")]
        public async Task<IActionResult> GetParcels()
        {
            var parcels = await _context.Parcels
                .Select(p => new {
                    p.TrackingId,
                    p.RecipientName,
                    p.DeliveryAddress,
                    p.Status,
                    p.CurrentLocation,
                    p.CreatedAt,
                    p.Weight,
                    SenderEmail = _context.Users
                        .Where(u => u.Id == p.SenderId)
                        .Select(u => u.Email)
                        .FirstOrDefault()
                })
                .ToListAsync();

            return Ok(parcels);
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