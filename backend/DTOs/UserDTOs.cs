namespace backend.DTOs
{
    public class CreateUserDto
    {
        public string FullName { get; set; } = null!;
        public string Email { get; set; } = null!;
        public string Role { get; set; } = "EMPLOYEE";
        public string Password { get; set; } = null!;
    }

    public class UpdateUserDto
    {
        public string FullName { get; set; } = null!;
        public string Email { get; set; } = null!;
        public string Role { get; set; } = "EMPLOYEE";
        public bool Status { get; set; } = true;
        public string? Password { get; set; }
    }
}
