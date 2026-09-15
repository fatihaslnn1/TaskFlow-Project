namespace TaskFlow.Domain.Entities
{
    public class Attachment
    {
        public int Id { get; set; }
        public string FileName { get; set; } = string.Empty;
        public string FilePath { get; set; } = string.Empty;
        public string ContentType { get; set; } = string.Empty;
        public long FileSize { get; set; }
        
        public int? IssueId { get; set; }
        public Issue? Issue { get; set; }
        
        public int? CommentId { get; set; }
        public Comment? Comment { get; set; }
        
        public int UserId { get; set; }
        public User? User { get; set; }
        
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}