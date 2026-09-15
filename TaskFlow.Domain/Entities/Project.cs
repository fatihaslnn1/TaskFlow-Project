namespace TaskFlow.Domain.Entities
{
    public class Project : ISoftDeletable
    {
        public int Id { get; set; }
        public string ProjectName { get; set; } = string.Empty;
        public string ProjectKey { get; set; } = string.Empty; // Örn: 'ACB'
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // ISoftDeletable Alanları (25. Madde - Soft Delete)
        public bool IsDeleted { get; set; } = false;
        public DateTime? DeletedAt { get; set; }
        public int? DeletedBy { get; set; }
    }
}