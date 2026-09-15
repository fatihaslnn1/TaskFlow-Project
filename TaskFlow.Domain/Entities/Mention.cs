namespace TaskFlow.Domain.Entities
{
    public class Mention
    {
        public int Id { get; set; }
        public int UserId { get; set; } // Kimden bahsedildi?
        public User? User { get; set; }
        
        public int? IssueId { get; set; } // Hangi görevde?
        public Issue? Issue { get; set; }
        
        public int? CommentId { get; set; } // Hangi yorumda?
        public Comment? Comment { get; set; }
        
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}