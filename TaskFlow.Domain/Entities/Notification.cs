namespace TaskFlow.Domain.Entities
{
    public class Notification
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public User? User { get; set; }
        public string Title { get; set; } = string.Empty;   // Bildirim başlığı
        public string Message { get; set; } = string.Empty; // Detay mesajı
        public string Type { get; set; } = string.Empty;   // Türü (Assigned, Mentioned, StatusChanged, DueDateApproaching, Overdued, NewComment, FeatureRequestResult)
        public int? RelatedIssueId { get; set; }          // İlgili Issue ID'si (varsa)
        public bool IsRead { get; set; } = false;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}