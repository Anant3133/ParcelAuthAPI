using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ParcelAuthAPI.Data;
using ParcelAuthAPI.DTOs; 
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace ParcelAuthAPI.Controllers
{
    [Authorize(Roles = "Sender,Handler,Admin")]
    [ApiController]
    [Route("api/[controller]")]
    public class ProfileController : ControllerBase
    {
        private readonly AppDbContext _context;

        public ProfileController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet("userinfo")]
        public async Task<IActionResult> GetUserInfo()
        {
            string userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            var user = await _context.Users.FindAsync(userId);
            if (user == null) return NotFound();

            var parcels = _context.Parcels
                .Where(p => p.SenderId == userId)
                .Select(p => new
                {
                    p.TrackingId,
                    p.RecipientName,
                    p.DeliveryAddress,
                    p.Status,
                    p.CreatedAt
                }).ToList();

            return Ok(new
            {
                user.Id,
                user.Email,
                user.Name,
                user.Role,
                Parcels = parcels
            });
        }

        [HttpPut]
        public async Task<IActionResult> UpdateProfile([FromBody] UserUpdateDto updatedUser)
        {
            string userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (userId != updatedUser.Id)
                return Forbid();

            var user = await _context.Users.FindAsync(userId);
            if (user == null) return NotFound();

            user.Name = updatedUser.Name;

            _context.Users.Update(user);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Profile updated successfully" });
        }
    }
}