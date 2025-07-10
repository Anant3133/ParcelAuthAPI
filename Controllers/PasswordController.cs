using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ParcelAuthAPI.Data;
using ParcelAuthAPI.DTOs;
using ParcelAuthAPI.Models;
using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace ParcelAuthAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PasswordController : ControllerBase
    {
        private readonly AppDbContext _context;

        public PasswordController(AppDbContext context)
        {
            _context = context;
        }

        [Authorize]
        [HttpPost("change")]
        public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordDto dto)
        {
            string userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var user = await _context.Users.FindAsync(userId);
            if (user == null) return NotFound();

            if (!BCrypt.Net.BCrypt.Verify(dto.OldPassword, user.PasswordHash))
                return BadRequest("Incorrect current password.");

            user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.NewPassword);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Password changed successfully" });
        }

        [HttpPost("forgot")]
        public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordDto dto)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == dto.Email);
            if (user == null)
                return Ok(new { message = "If this email exists, a reset link will be sent." });

            user.ResetToken = Guid.NewGuid().ToString();
            user.ResetTokenExpiry = DateTime.UtcNow.AddMinutes(30);
            await _context.SaveChangesAsync();

            // Normally you'd send this via email:
            return Ok(new
            {
                message = "Password reset token generated.",
                token = user.ResetToken // 🔐 expose only for testing; don't return this in prod
            });
        }

        [HttpPost("reset")]
        public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordDto dto)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u =>
                u.ResetToken == dto.Token && u.ResetTokenExpiry > DateTime.UtcNow);

            if (user == null)
                return BadRequest("Invalid or expired reset token.");

            user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.NewPassword);
            user.ResetToken = null;
            user.ResetTokenExpiry = null;

            await _context.SaveChangesAsync();

            return Ok(new { message = "Password has been reset successfully." });
        }
    }
}
