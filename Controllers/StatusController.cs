using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using ParcelAuthAPI.Models;
using ParcelAuthAPI.Data;
using System.Threading.Tasks;
using System;

namespace ParcelAuthAPI.Controllers
{
    [Authorize(Roles = "Admin,Handler,Sender")]
    [ApiController]
    [Route("api/[controller]")]
    public class StatusController : ControllerBase
    {
        private readonly AppDbContext _context;

        public StatusController(AppDbContext context)
        {
            _context = context;
        }

        public class UpdateStatusRequest
        {
            public string ParcelTrackingId { get; set; } = string.Empty;
            public string Status { get; set; } = string.Empty;
        }

        [HttpPost("update")]
        public async Task<IActionResult> UpdateStatus([FromBody] UpdateStatusRequest request)
        {
            string userId = User.FindFirst("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier")?.Value;
            string role = User.FindFirst("http://schemas.microsoft.com/ws/2008/06/identity/claims/role")?.Value;

            Console.WriteLine("User ID: " + userId);
            Console.WriteLine("User Role: " + role);

            if (string.IsNullOrWhiteSpace(request.ParcelTrackingId) || string.IsNullOrWhiteSpace(request.Status))
            {
                return BadRequest("ParcelTrackingId and Status are required.");
            }

            var parcel = await _context.Parcels.FindAsync(request.ParcelTrackingId);
            if (parcel == null)
            {
                return NotFound($"Parcel with TrackingId '{request.ParcelTrackingId}' not found.");
            }

            
            parcel.Status = request.Status;
            _context.Parcels.Update(parcel);

            
            var statusLog = new ParcelStatusLog
            {
                ParcelTrackingId = request.ParcelTrackingId,
                Status = request.Status,
                Timestamp = DateTime.UtcNow
            };

            await _context.ParcelStatusLogs.AddAsync(statusLog);
            await _context.SaveChangesAsync();

            return Ok(new { message = $"Status updated to '{request.Status}' for parcel '{request.ParcelTrackingId}'." });
        }
    }
}