using System.Security.Claims;
using FitnessApi.DTOs;
using FitnessApi.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FitnessApi.Controllers
{
    [Route("api/goals")]
    [ApiController]
    [Authorize]
    public class GoalsController : ControllerBase
    {
        private readonly IGoalsService _goalsService;

        public GoalsController(IGoalsService goalsService)
        {
            _goalsService = goalsService;
        }

        [HttpGet("me")]
        public async Task<IActionResult> GetGoal()
        {
            var userId = GetUserId();
            if (userId == null) return Unauthorized();
            return Ok(await _goalsService.GetGoalAsync(userId.Value));
        }

        [HttpPut("me")]
        public async Task<IActionResult> UpdateGoal([FromBody] GoalDto dto)
        {
            if (dto.WeeklyGoal < 1 || dto.WeeklyGoal > 7)
                return BadRequest("Weekly goal must be between 1 and 7.");
            var userId = GetUserId();
            if (userId == null) return Unauthorized();
            await _goalsService.UpdateGoalAsync(userId.Value, dto.WeeklyGoal);
            return NoContent();
        }

        [HttpGet("achievements")]
        public async Task<IActionResult> GetAchievements()
        {
            var userId = GetUserId();
            if (userId == null) return Unauthorized();
            return Ok(await _goalsService.GetAchievementsAsync(userId.Value));
        }

        private Guid? GetUserId()
        {
            var str = User.FindFirstValue(ClaimTypes.NameIdentifier);
            return Guid.TryParse(str, out var id) ? id : null;
        }
    }
}
