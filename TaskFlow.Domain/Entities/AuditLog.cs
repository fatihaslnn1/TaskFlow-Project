namespace TaskFlow.Domain.Entities
{
    public class AuditLog
    {
        public int Id { get; set; }
        public int IssueId { get; set; }
        public int UserId { get; set; }
        public string Action { get; set; } = string.Empty;      // "StatusChanged", "Assigned", "CommentAdded" vb.
        public string Details { get; set; } = string.Empty;     // "Durum To Do -> In Progress olarak değiştirildi." vb.
        public DateTime CreatedAt { get; set; }

        // Navigation Property (İsteğe bağlı, EF Core'un ilişkileri anlaması için)
        public Issue? Issue { get; set; }
    }
}