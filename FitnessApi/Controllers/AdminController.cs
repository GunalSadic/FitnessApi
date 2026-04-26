using FitnessApi.Database;
using FitnessApi.DTOs;
using FitnessApi.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FitnessApi.Controllers
{
    [Route("api/admin")]
    [ApiController]
    [Authorize(Roles = "Admin")]
    public class AdminController : ControllerBase
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly FitnessDbContext _context;

        public AdminController(UserManager<ApplicationUser> userManager, FitnessDbContext context)
        {
            _userManager = userManager;
            _context = context;
        }

        [HttpPost("checkin")]
        public async Task<IActionResult> CheckIn([FromBody] CheckInDto dto)
        {
            if (!Guid.TryParse(dto.UserId, out var userId))
                return BadRequest(new CheckInResultDto { IsSuccess = false, Message = "Invalid user ID." });

            var user = await _userManager.FindByIdAsync(dto.UserId);
            if (user == null)
                return NotFound(new CheckInResultDto { IsSuccess = false, Message = "User not found." });

            var now = DateTime.UtcNow;
            var hasActiveSubscription = await _context.Subscriptions
                .AnyAsync(s => s.UserId == userId && s.EndDate >= now);

            _context.AccessLogs.Add(new AccessLog
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                ScanTime = now,
                ScanLocation = "Main Entrance",
                WasAccessGranted = hasActiveSubscription
            });

            await _context.SaveChangesAsync();

            return Ok(new CheckInResultDto
            {
                IsSuccess = true,
                Message = hasActiveSubscription ? "Access granted." : "Access denied — no active subscription.",
                UserFullName = user.FullName,
                HasActiveSubscription = hasActiveSubscription
            });
        }
    }
}
