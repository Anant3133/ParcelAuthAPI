using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ParcelAuthAPI.Data;
using ParcelAuthAPI.Models;
using ParcelAuthAPI.Services;
using System.Security.Claims;

namespace ParcelAuthAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TamperAlertController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly INotificationService _emailService;

        public TamperAlertController(AppDbContext context, INotificationService emailService)
        {
            _context = context;
            _emailService = emailService;
        }

        [HttpPost("raise")]
        [Authorize(Roles = "Handler , Admin")]
        public async Task<IActionResult> RaiseAlert([FromBody] TamperAlert alert)
        {
            if (string.IsNullOrWhiteSpace(alert.ParcelTrackingId))
                return BadRequest("Tracking ID is required.");

            alert.HandlerId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "Unknown";
            alert.Timestamp = DateTime.UtcNow;

            _context.TamperAlerts.Add(alert);
            await _context.SaveChangesAsync();

            var parcel = await _context.Parcels
    .FirstOrDefaultAsync(p => p.TrackingId.ToLower() == alert.ParcelTrackingId.ToLower());

            string toEmail = parcel?.SenderEmail ?? "yourbackupadmin@email.com";
            string subject = $"Tamper Alert for Parcel {alert.ParcelTrackingId}";
            string body = $"A tamper alert was raised:\n\nMessage: {alert.Message}\nTime: {alert.Timestamp}";

            try
            {
                await _emailService.SendEmailAsync(toEmail, subject, body);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Failed to send tamper email: {ex.Message}");
            }

            return Ok(alert);
        }
        [HttpGet("all")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetAllAlertsWithParcels()
        {
            var enrichedAlerts = await _context.TamperAlerts
                .OrderByDescending(a => a.Timestamp)
                .GroupJoin(
                    _context.Parcels,
                    alert => alert.ParcelTrackingId.Trim().ToLower(),
                    parcel => parcel.TrackingId.Trim().ToLower(),
                    (alert, parcels) => new { alert, parcels }
                )
                .SelectMany(
                    ap => ap.parcels.DefaultIfEmpty(),
                    (ap, parcel) => new
                    {
                        ap.alert.Id,
                        ap.alert.ParcelTrackingId,
                        ap.alert.Message,
                        ap.alert.Timestamp,
                        Parcel = parcel == null ? null : new
                        {
                            parcel.RecipientName,
                            parcel.DeliveryAddress,
                            parcel.Status,
                            parcel.CurrentLocation,
                            parcel.Weight
                        }
                    }
                )
                .ToListAsync();

            return Ok(enrichedAlerts);
        }


        [HttpGet("{TrackingId}")]
        [Authorize]
        public IActionResult GetAlertsForParcel(string TrackingId)
        {
            var alerts = _context.TamperAlerts
                .Where(a => a.ParcelTrackingId == TrackingId)
                .OrderByDescending(a => a.Timestamp)
                .ToList();

            return Ok(alerts);
        }

        [HttpGet("my")]
        [Authorize(Roles = "Handler , Admin")]
        public IActionResult GetMyRaisedAlerts()
        {
            var handlerId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var alerts = _context.TamperAlerts
                .Where(a => a.HandlerId == handlerId)
                .OrderByDescending(a => a.Timestamp)
                .ToList();

            return Ok(alerts);
        }
    }
}