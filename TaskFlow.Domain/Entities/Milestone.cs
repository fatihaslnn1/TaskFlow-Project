namespace TaskFlow.Domain.Entities
{
    public class Milestone
    {
        public int Id { get; set; }
        public int ProjectId { get; set; }
        public string Title { get; set; } = string.Empty;       // "Beta Sürüm Çıkışı"
        public string Description { get; set; } = string.Empty;
        public DateTime DueDate { get; set; }
        public bool IsCompleted { get; set; }

        public Project? Project { get; set; }
        public ICollection<Issue> Issues { get; set; } = new List<Issue>();
    }
}