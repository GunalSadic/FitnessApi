namespace FitnessApi.Models
{
    public class AccessLog
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public DateTime ScanTime { get; set; }
        public string ScanLocation { get; set; }
        public bool WasAccessGranted { get; set; }
    }
}
