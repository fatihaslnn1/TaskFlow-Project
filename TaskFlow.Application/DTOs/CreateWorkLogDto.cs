namespace TaskFlow.Application.DTOs
{
    public class CreateWorkLogDto
    {
        public int IssueId { get; set; }
        public decimal HoursSpent { get; set; }
        public string Description { get; set; } = string.Empty;
    }
}