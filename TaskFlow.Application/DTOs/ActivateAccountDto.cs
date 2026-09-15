namespace TaskFlow.Application.DTOs;

public class ActivateAccountDto
{
    public string ActivationToken { get; set; } = string.Empty;
    public string NewPassword { get; set; } = string.Empty;
}