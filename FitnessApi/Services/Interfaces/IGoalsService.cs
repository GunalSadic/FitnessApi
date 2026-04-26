using FitnessApi.DTOs;

namespace FitnessApi.Services.Interfaces
{
    public interface IGoalsService
    {
        Task<GoalDto> GetGoalAsync(Guid userId);
        Task UpdateGoalAsync(Guid userId, int weeklyGoal);
        Task<List<AchievementDto>> GetAchievementsAsync(Guid userId);
    }
}
