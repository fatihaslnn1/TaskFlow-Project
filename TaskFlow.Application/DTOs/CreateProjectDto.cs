namespace TaskFlow.Application.DTOs;

public class CreateProjectDto
{
    public string ProjectName { get; set; } = string.Empty;
    public string ProjectKey { get; set; } = string.Empty; // Örn: TASK, JIRA
}