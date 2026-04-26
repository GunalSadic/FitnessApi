using System.Globalization;
using FitnessApi.Database;
using FitnessApi.DTOs;
using FitnessApi.Models;
using FitnessApi.Services.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace FitnessApi.Services
{
    public class GoalsService : IGoalsService
    {
        private readonly FitnessDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public GoalsService(FitnessDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public async Task<GoalDto> GetGoalAsync(Guid userId)
        {
            var user = await _userManager.FindByIdAsync(userId.ToString());
            return new GoalDto { WeeklyGoal = user?.WeeklyGoal ?? 3 };
        }

        public async Task UpdateGoalAsync(Guid userId, int weeklyGoal)
        {
            var user = await _userManager.FindByIdAsync(userId.ToString());
            if (user == null) return;
            user.WeeklyGoal = weeklyGoal;
            await _userManager.UpdateAsync(user);
        }

        public async Task<List<AchievementDto>> GetAchievementsAsync(Guid userId)
        {
            var user = await _userManager.FindByIdAsync(userId.ToString());
            var weeklyGoal = user?.WeeklyGoal ?? 3;

            var allScanTimes = await _context.AccessLogs
                .Where(a => a.UserId == userId && a.WasAccessGranted)
                .Select(a => a.ScanTime)
                .ToListAsync();

            var totalVisits = allScanTimes.Count;
            var sortedDates = allScanTimes.Select(t => t.Date).Distinct().OrderBy(d => d).ToList();
            var bestStreak = ComputeBestStreak(sortedDates);
            var consecutiveGoalWeeks = ComputeConsecutiveGoalWeeks(sortedDates, weeklyGoal);

            var stats = new UserStats(totalVisits, bestStreak, consecutiveGoalWeeks);

            return Achievements.Select(a => new AchievementDto
            {
                Id = a.Id,
                Name = a.Name,
                Description = a.Description,
                Icon = a.Icon,
                IsUnlocked = a.Check(stats)
            }).ToList();
        }

        // ── Achievement definitions ──────────────────────────────────────────

        private record Achievement(string Id, string Name, string Description, string Icon, Func<UserStats, bool> Check);
        private record UserStats(int TotalVisits, int BestStreak, int ConsecutiveGoalWeeks);

        private static readonly List<Achievement> Achievements =
        [
            new("first_visit",  "First Step",        "Complete your first gym visit",                  "⭐", s => s.TotalVisits >= 1),
            new("visits_10",    "Getting Started",   "Complete 10 gym visits",                         "🏅", s => s.TotalVisits >= 10),
            new("visits_50",    "Dedicated",         "Complete 50 gym visits",                         "🥈", s => s.TotalVisits >= 50),
            new("visits_100",   "Centurion",         "Complete 100 gym visits",                        "🥇", s => s.TotalVisits >= 100),
            new("streak_7",     "Week Warrior",      "Achieve a 7-day check-in streak",                "🔥", s => s.BestStreak >= 7),
            new("streak_30",    "Month Master",      "Achieve a 30-day check-in streak",               "💎", s => s.BestStreak >= 30),
            new("goal_1week",   "Goal Getter",       "Hit your weekly goal for 1 week",                "🎯", s => s.ConsecutiveGoalWeeks >= 1),
            new("goal_4weeks",  "Four Weeks Strong", "Hit your weekly goal for 4 consecutive weeks",   "💪", s => s.ConsecutiveGoalWeeks >= 4),
            new("goal_12weeks", "Unstoppable",       "Hit your weekly goal for 12 consecutive weeks",  "👑", s => s.ConsecutiveGoalWeeks >= 12),
        ];

        // ── Helpers ──────────────────────────────────────────────────────────

        private static int ComputeBestStreak(List<DateTime> sortedDates)
        {
            if (sortedDates.Count == 0) return 0;
            int best = 1, current = 1;
            for (int i = 1; i < sortedDates.Count; i++)
            {
                if ((sortedDates[i] - sortedDates[i - 1]).TotalDays == 1)
                {
                    current++;
                    if (current > best) best = current;
                }
                else
                {
                    current = 1;
                }
            }
            return best;
        }

        private static int ComputeConsecutiveGoalWeeks(List<DateTime> visitDates, int weeklyGoal)
        {
            if (weeklyGoal == 0 || visitDates.Count == 0) return 0;

            var visitsByWeek = visitDates
                .GroupBy(GetWeekKey)
                .ToDictionary(g => g.Key, g => g.Count());

            // Walk backwards from the current calendar week (Mon–Sun)
            var today = DateTime.UtcNow.Date;
            int daysFromMonday = ((int)today.DayOfWeek + 6) % 7;
            var weekStart = today.AddDays(-daysFromMonday);

            int consecutive = 0;
            while (true)
            {
                var key = GetWeekKey(weekStart);
                if (visitsByWeek.TryGetValue(key, out var count) && count >= weeklyGoal)
                {
                    consecutive++;
                    weekStart = weekStart.AddDays(-7);
                }
                else
                    break;
            }
            return consecutive;
        }

        private static (int Year, int Week) GetWeekKey(DateTime date)
        {
            var cal = CultureInfo.InvariantCulture.Calendar;
            return (date.Year, cal.GetWeekOfYear(date, CalendarWeekRule.FirstFourDayWeek, DayOfWeek.Monday));
        }
    }
}
