namespace FitnessApi.DTOs
{
    public class SubscriptionDto
    {
        public bool IsActive { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public int? DaysRemaining { get; set; }
    }
}
