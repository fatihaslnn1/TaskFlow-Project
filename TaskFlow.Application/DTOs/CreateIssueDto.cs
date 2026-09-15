namespace TaskFlow.Application.DTOs;

public class CreateIssueDto
{
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string IssueType { get; set; } = "Task"; // Task, Bug, Feature Request
    public string Priority { get; set; } = "Medium"; // Low, Medium, High
    public int ProjectId { get; set; }
    public int? AssigneeId { get; set; }
    
    public DateTime? DueDate { get; set; }
    public decimal? EstimatedEffort { get; set; }
}