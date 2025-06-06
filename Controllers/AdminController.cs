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
        public async Task<IActionResult> GetParcels()
        {
            var parcels = await _context.Parcels.ToListAsync();
            return Ok(parcels);
        }

        [HttpGet("users")]
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
