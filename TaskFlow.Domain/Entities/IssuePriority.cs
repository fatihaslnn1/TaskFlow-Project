namespace TaskFlow.Domain.Entities
{
    public class IssuePriority
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty; // Örn: Low, Medium, High, Critical
    }
}