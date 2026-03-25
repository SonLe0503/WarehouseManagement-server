namespace warehouseManagement.DTOs.DashboardAdmin
{
    public class RecentUserDto
    {
        public int Id { get; set; }
        public string Username { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public List<string> Roles { get; set; } = new();
        public string? WarehouseName { get; set; }
        public DateTime? CreatedAt { get; set; }
    }
}
