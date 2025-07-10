namespace ResumeManagement.Models
{
    public class Log
    {
        public Guid Id { get; set; }
        public string Action { get; set; } = null!;
        public string PerformedBy { get; set; } = null!;
        public DateTime Timestamp { get; set; }
        public string Details { get; set; } = null!;
    }

}
