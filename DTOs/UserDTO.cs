namespace warehouseManagement.DTOs
{
    public class UserDTO
    {
        public int Id { get; set; }
        public string Username { get; set; }
        public string Email { get; set; }
        public string Status { get; set; }
        public List<string> Roles { get; set; }
        public DateTime? CreatedAt { get; set; }
    }

    public class CreateUserDTO
    {
        public string Username { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
        public string Status { get; set; }
        public List<int> RoleIds { get; set; }
    }

    public class UpdateUserDTO
    {
        public string Username { get; set; }
        public string Email { get; set; }
        public string Status { get; set; }
        public List<int> RoleIds { get; set; }
    }
}
