namespace TaskFlow.Domain.Entities
{
    public class Sprint
    {
        public int Id { get; set; }
        public int ProjectId { get; set; }
        public string Name { get; set; } = string.Empty;       // "Sprint 1", "MVP Sürümü" vb.
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string Goal { get; set; } = string.Empty;        // Sprint Hedefi
        public string Status { get; set; } = "Planned";        // Planned, Active, Completed

        public Project? Project { get; set; }
        
        
        public ICollection<Issue> Issues { get; set; } = new List<Issue>();

        
        public ICollection<SprintIssue> SprintIssues { get; set; } = new List<SprintIssue>();
    }
}