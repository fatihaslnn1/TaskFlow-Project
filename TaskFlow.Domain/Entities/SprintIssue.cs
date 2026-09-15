namespace TaskFlow.Domain.Entities
{
    public class SprintIssue
    {
        public int SprintId { get; set; }
        public Sprint? Sprint { get; set; }
        
        public int IssueId { get; set; }
        public Issue? Issue { get; set; }
        
        public DateTime AddedAt { get; set; } = DateTime.UtcNow;
    }
}