using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ParcelAuthAPI.Data;
using ParcelAuthAPI.Models;
using System.Security.Claims;

namespace ParcelAuthAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TamperAlertController : ControllerBase
    {
        private readonly AppDbContext _context;

        public TamperAlertController(AppDbContext context)
        {
            _context = context;
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

            return Ok(alert);
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