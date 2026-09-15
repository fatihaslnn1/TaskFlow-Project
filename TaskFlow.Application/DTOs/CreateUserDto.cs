namespace TaskFlow.Application.DTOs
{
    public class CreateUserDto
    {
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public int RoleId { get; set; } // 1: Admin, 2: PM, 3: Developer, 4: Reporter, 5: Customer
    }
}