using FitnessApi.DTOs;

namespace FitnessApi.Services.Interfaces
{
    public interface ISubscriptionService
    {
        Task<SubscriptionDto> GetActiveSubscriptionAsync(Guid userId);
    }
}
