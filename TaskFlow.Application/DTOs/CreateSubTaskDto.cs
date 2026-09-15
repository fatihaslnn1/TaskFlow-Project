namespace TaskFlow.Application.DTOs
{
    public class CreateSubTaskDto
    {
        public int IssueId { get; set; }
        public string Title { get; set; } = string.Empty;
    }
}