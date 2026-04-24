using Microsoft.AspNetCore.Identity;

namespace FitnessApi.Models
{
    public class ApplicationUser : IdentityUser<Guid>
    {
        public string FullName { get; set; }
        public ICollection<Subscription> Subscriptions { get; set; }
    }
}
