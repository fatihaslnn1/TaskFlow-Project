namespace TaskFlow.Domain.Entities
{
    public class User
    {
        public int Id { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string PasswordHash { get; set; } = string.Empty;
        public bool IsActive { get; set; } = false;
        public string? ActivationToken { get; set; }
        
        public int RoleId { get; set; }
        public Role? Role { get; set; }
    }
}