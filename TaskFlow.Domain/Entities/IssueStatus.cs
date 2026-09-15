namespace TaskFlow.Domain.Entities
{
    public class IssueStatus
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty; // Örn: Todo, In Progress, Done
    }
}