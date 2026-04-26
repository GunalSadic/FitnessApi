using FitnessApi.Database;
using FitnessApi.DTOs;
using FitnessApi.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace FitnessApi.Services
{
    public class StatsService : IStatsService
    {
        private readonly FitnessDbContext _context;

        public StatsService(FitnessDbContext context)
        {
            _context = context;
        }

        public async Task<StatsDto> GetStatsAsync(Guid userId)
        {
            var now = DateTime.UtcNow;
            var startOfMonth = new DateTime(now.Year, now.Month, 1, 0, 0, 0, DateTimeKind.Utc);
            var startOfNextMonth = startOfMonth.AddMonths(1);

            var visitsThisMonth = await _context.AccessLogs
                .CountAsync(a => a.UserId == userId
                    && a.WasAccessGranted
                    && a.ScanTime >= startOfMonth
                    && a.ScanTime < startOfNextMonth);

            var allScanTimes = await _context.AccessLogs
                .Where(a => a.UserId == userId && a.WasAccessGranted)
                .Select(a => a.ScanTime)
                .ToListAsync();

            var dateSet = allScanTimes
                .Select(t => t.Date)
                .ToHashSet();

            var sortedDates = dateSet.OrderBy(d => d).ToList();

            return new StatsDto
            {
                CurrentStreak = ComputeCurrentStreak(dateSet, now.Date),
                BestStreak = ComputeBestStreak(sortedDates),
                VisitsThisMonth = visitsThisMonth
            };
        }

        private static int ComputeCurrentStreak(HashSet<DateTime> dateSet, DateTime today)
        {
            // Accept streak ending today or yesterday (user may not have visited yet today)
            var cursor = dateSet.Contains(today) ? today : today.AddDays(-1);
            if (!dateSet.Contains(cursor)) return 0;

            int streak = 0;
            while (dateSet.Contains(cursor))
            {
                streak++;
                cursor = cursor.AddDays(-1);
            }
            return streak;
        }

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
    }
}
