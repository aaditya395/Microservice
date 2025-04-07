namespace NotificationService.Models
{
    public class Notification
    {
        public int Id { get; set; }
        public int EmployeeId { get; set; }
        public string Message { get; set; }
        public string Email { get; set; }
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    }
}
