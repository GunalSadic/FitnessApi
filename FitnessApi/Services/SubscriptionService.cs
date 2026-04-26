using FitnessApi.Database;
using FitnessApi.DTOs;
using FitnessApi.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace FitnessApi.Services
{
    public class SubscriptionService : ISubscriptionService
    {
        private readonly FitnessDbContext _context;

        public SubscriptionService(FitnessDbContext context)
        {
            _context = context;
        }

        public async Task<SubscriptionDto> GetActiveSubscriptionAsync(Guid userId)
        {
            var now = DateTime.UtcNow;

            var active = await _context.Subscriptions
                .Where(s => s.UserId == userId && s.EndDate >= now)
                .OrderByDescending(s => s.EndDate)
                .FirstOrDefaultAsync();

            if (active != null)
            {
                return new SubscriptionDto
                {
                    IsActive = true,
                    StartDate = active.StartDate,
                    EndDate = active.EndDate,
                    DaysRemaining = (int)(active.EndDate - now).TotalDays
                };
            }

            var expired = await _context.Subscriptions
                .Where(s => s.UserId == userId)
                .OrderByDescending(s => s.EndDate)
                .FirstOrDefaultAsync();

            if (expired != null)
            {
                return new SubscriptionDto
                {
                    IsActive = false,
                    StartDate = expired.StartDate,
                    EndDate = expired.EndDate,
                    DaysRemaining = null
                };
            }

            return new SubscriptionDto { IsActive = false };
        }
    }
}
