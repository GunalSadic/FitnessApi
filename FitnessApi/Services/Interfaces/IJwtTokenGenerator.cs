using FitnessApi.Models;

namespace FitnessApi.Services.Interfaces
{
    public interface IJwtTokenGenerator
    {
        public Task<string> GenerateJwtToken(ApplicationUser user);
        
    }
}
