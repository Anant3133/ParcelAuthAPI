using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ParcelAuthAPI.Data;
using ParcelAuthAPI.Models;
using System.Linq;
using System.Threading.Tasks;

namespace ParcelAuthAPI.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class RouteController : ControllerBase
    {
        private readonly AppDbContext _context;

        public RouteController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet("{TrackingId}")]
        public async Task<IActionResult> GetRoute(string TrackingId)
        {
            if (string.IsNullOrWhiteSpace(TrackingId))
                return BadRequest("TrackingId cannot be empty");

            // Find the parcel
            var parcel = _context.Parcels
                .Where(p => p.TrackingId == TrackingId)
                .Select(p => new
                {
                    p.SenderAddress,
                    p.DeliveryAddress
                })
                .FirstOrDefault();

            if (parcel == null)
                return NotFound("Parcel not found");

            return Ok(new
            {
                senderAddress = parcel.SenderAddress,
                deliveryAddress = parcel.DeliveryAddress
            });
        }
    }
}