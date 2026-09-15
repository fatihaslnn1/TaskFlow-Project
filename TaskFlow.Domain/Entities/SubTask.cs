namespace TaskFlow.Domain.Entities
{
    public class SubTask
    {
        public int Id { get; set; }
        
        public int IssueId { get; set; }
        public string Title { get; set; } = string.Empty;
        public bool IsCompleted { get; set; }

        
        public Issue? Issue { get; set; } 
    }
}