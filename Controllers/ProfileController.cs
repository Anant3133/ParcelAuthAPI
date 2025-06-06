using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using ParcelAuthAPI.Data;
using System.Threading.Tasks;
using System.Linq;

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
            string userId = User.FindFirst("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier")?.Value;
            var user = await _context.Users.FindAsync(userId);
            if (user == null) return NotFound();

            var parcels = _context.Parcels
                .Where(p => p.SenderId == userId)
                .Select(p => new {
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
                user.Role,
                Parcels = parcels
            });
        }
    }
}