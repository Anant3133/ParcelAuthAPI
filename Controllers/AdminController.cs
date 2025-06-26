using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ParcelAuthAPI.Data;
using ParcelAuthAPI.Models;

namespace ParcelAuthAPI.Controllers
{
    [Authorize(Roles = "Admin")]
    [ApiController]
    [Route("api/[controller]")]
    public class AdminController : ControllerBase
    {
        private readonly AppDbContext _context;

        public AdminController(AppDbContext context)
        {
            _context = context;
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



        [HttpGet("users")]
        [Authorize(Roles = "Admin , Handler")]
        public async Task<IActionResult> GetUsers()
        {
            var users = await _context.Users.Select(u => new {
                u.Id,
                u.Email,
                u.Role
            }).ToListAsync();
            return Ok(users);
        }

        [HttpGet("alerts")]
        public async Task<IActionResult> GetTamperAlerts()
        {
            var alerts = await _context.TamperAlerts.ToListAsync();
            return Ok(alerts);
        }

        [HttpGet("stats")]
        public async Task<IActionResult> GetStats()
        {
            var stats = new
            {
                TotalParcels = await _context.Parcels.CountAsync(),
                TotalUsers = await _context.Users.CountAsync(),
                TotalAlerts = await _context.TamperAlerts.CountAsync()
            };
            return Ok(stats);
        }
    }
}
