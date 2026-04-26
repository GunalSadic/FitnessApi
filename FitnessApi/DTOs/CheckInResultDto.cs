namespace FitnessApi.DTOs
{
    public class CheckInResultDto
    {
        public bool IsSuccess { get; set; }
        public string Message { get; set; } = string.Empty;
        public string? UserFullName { get; set; }
        public bool HasActiveSubscription { get; set; }
    }
}
