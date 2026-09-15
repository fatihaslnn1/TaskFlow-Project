namespace TaskFlow.Domain.Entities
{
    public class IssueType
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty; // Örn: Bug, Task, Feature
    }
}