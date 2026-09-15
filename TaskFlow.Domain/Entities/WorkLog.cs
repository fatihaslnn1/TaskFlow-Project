namespace TaskFlow.Domain.Entities
{
    public class WorkLog
    {
        public int Id { get; set; }
        public int IssueId { get; set; }
        public int UserId { get; set; }
        public decimal HoursSpent { get; set; }
        public string Description { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }
}