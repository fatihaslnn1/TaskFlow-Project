namespace TaskFlow.Domain.Entities
{
    public class FeatureRequest
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Status { get; set; } = "Pending"; // Pending, Approved, Rejected vb.
        public int UserId { get; set; }
        public User? User { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public ICollection<AcceptanceCriteria> AcceptanceCriterias { get; set; } = new List<AcceptanceCriteria>();
    }
}