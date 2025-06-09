using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ParcelAuthAPI.Data;
using System.Linq;
using System.Threading.Tasks;

namespace ParcelAuthAPI.Controllers
{
    [Authorize(Roles = "Sender,Handler,Admin")]
    [ApiController]
    [Route("api/[controller]")]
    public class TimelineController : ControllerBase
    {
        private readonly AppDbContext _context;

        public TimelineController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet("{trackingId}")]
        public async Task<IActionResult> GetTimeline(string trackingId)
        {
            var parcel = await _context.Parcels.FirstOrDefaultAsync(p => p.TrackingId == trackingId);
            if (parcel == null) return NotFound();

            var statusLogs = _context.ParcelStatusLogs
                .Where(s => s.ParcelTrackingId == trackingId)
                .OrderBy(s => s.Timestamp)
                .Select(s => new { s.Status, s.Timestamp })
                .ToList();

            var handovers = _context.Handovers
                .Where(h => h.ParcelTrackingId == trackingId)
                .OrderBy(h => h.HandoverTime)
                .Select(h => new { h.HandlerId, h.HandoverTime })
                .ToList();

            return Ok(new
            {
                Parcel = new
                {
                    parcel.TrackingId,
                    parcel.RecipientName,
                    parcel.DeliveryAddress,
                    parcel.Status,
                    parcel.CreatedAt
                },
                StatusLogs = statusLogs,
                Handovers = handovers
            });
        }
    }
}