using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ParcelAuthAPI.Data;
using ParcelAuthAPI.DTOs;
using ParcelAuthAPI.Models;
using ParcelAuthAPI.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace ParcelAuthAPI.Controllers
{
    [Authorize(Roles = "Sender,Handler,Admin")]
    [ApiController]
    [Route("api/[controller]")]
    public class StatusController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly INotificationService _notificationService;

        private readonly HashSet<string> _validStatuses = new()
        {
            "Received", "Packed", "Shipped", "Out for Delivery", "Delivered"
        };

        public StatusController(AppDbContext context, INotificationService notificationService)
        {
            _context = context;
            _notificationService = notificationService;
        }

        [HttpPost("update")]
        public async Task<IActionResult> UpdateStatus([FromBody] ParcelStatusDTO dto)
        {
            if (dto == null)
                return BadRequest("Request body cannot be empty.");

            if (string.IsNullOrWhiteSpace(dto.TrackingId))
                return BadRequest("TrackingId is required.");

            if (!_validStatuses.Contains(dto.Status))
                return BadRequest($"Invalid status. Must be one of: {string.Join(", ", _validStatuses)}.");

            var parcel = await _context.Parcels.FirstOrDefaultAsync(p => p.TrackingId == dto.TrackingId);
            if (parcel == null)
                return NotFound($"Parcel with TrackingId '{dto.TrackingId}' not found.");

            
            parcel.Status = dto.Status;

        
            var statusLog = new ParcelStatusLog
            {
                ParcelTrackingId = dto.TrackingId,
                Status = dto.Status,
                Timestamp = DateTime.UtcNow
            };
            _context.ParcelStatusLogs.Add(statusLog);

            
            if (dto.Status == "Delivered")
            {
                var sender = await _context.Users.FindAsync(parcel.SenderId);
                if (sender != null && !string.IsNullOrEmpty(sender.Email))
                {
                    await _notificationService.SendEmailAsync(
                        sender.Email,
                        "Parcel Delivered",
                        $"Your parcel {dto.TrackingId} has been successfully delivered."
                    );
                }
            }

            await _context.SaveChangesAsync();

            return Ok(new { message = $"Parcel {dto.TrackingId} status updated to '{dto.Status}'." });
        }

        [HttpGet("all")]
        public async Task<IActionResult> GetAllParcels()
        {
            var parcels = await _context.Parcels
                .Select(p => new
                {
                    p.TrackingId,
                    p.RecipientName,
                    p.DeliveryAddress,
                    p.Status,
                    p.CurrentLocation,
                    p.CreatedAt
                })
                .ToListAsync();

            return Ok(parcels);
        }

        [AllowAnonymous] 
        [HttpGet("{TrackingId}")]
        public async Task<IActionResult> GetStatusLogs(string TrackingId)
        {
            var logs = await _context.ParcelStatusLogs
                .Where(log => log.ParcelTrackingId == TrackingId)
                .OrderBy(log => log.Timestamp)
                .Select(log => new ParcelStatusDTO
                {
                    TrackingId = log.ParcelTrackingId,
                    Status = log.Status,
                    Timestamp = log.Timestamp
                })
                .ToListAsync();

            if (!logs.Any())
            {
                return NotFound($"No status logs found for tracking ID: {TrackingId}");
            }

            return Ok(logs);
        }
    }
}