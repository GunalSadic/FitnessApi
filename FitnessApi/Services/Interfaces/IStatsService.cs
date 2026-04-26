using FitnessApi.DTOs;

namespace FitnessApi.Services.Interfaces
{
    public interface IStatsService
    {
        Task<StatsDto> GetStatsAsync(Guid userId);
    }
}
