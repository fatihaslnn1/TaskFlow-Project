namespace TaskFlow.Application.DTOs
{
    public class SetPasswordDto
    {
        public string Token { get; set; } = string.Empty;
        public string NewPassword { get; set; } = string.Empty;
    }
}