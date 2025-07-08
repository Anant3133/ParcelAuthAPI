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
                user.Birthday,
                user.Mobile,
                user.Location,
                user.Languages,
                user.About,
                user.ProfilePictureUrl,
                user.BannerImageUrl,
                Parcels = parcels
            });
        }

        [HttpPost("upload-profile-picture")]
        public async Task<IActionResult> UploadProfilePicture([FromBody] UploadImageDTO dto)
        {
            string userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userId == null) return Unauthorized();

            var user = await _context.Users.FindAsync(userId);
            if (user == null) return NotFound();

            user.ProfilePictureUrl = dto.ImageUrl;
            _context.Users.Update(user);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Profile picture updated successfully" });
        }

        [HttpPost("upload-banner-image")]
        public async Task<IActionResult> UploadBannerImage([FromBody] UploadImageDTO dto)
        {
            string userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userId == null) return Unauthorized();

            var user = await _context.Users.FindAsync(userId);
            if (user == null) return NotFound();

            user.BannerImageUrl = dto.ImageUrl;
            _context.Users.Update(user);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Banner image updated successfully" });
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
            user.Birthday = updatedUser.Birthday;
            user.Mobile = updatedUser.Mobile;
            user.Location = updatedUser.Location;
            user.Languages = updatedUser.Languages;
            user.About = updatedUser.About;
            _context.Users.Update(user);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Profile updated successfully" });
        }
    }
}