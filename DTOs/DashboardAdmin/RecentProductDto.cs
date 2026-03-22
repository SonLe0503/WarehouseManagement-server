namespace warehouseManagement.DTOs.DashboardAdmin
{
    public class RecentProductDto
    {
        public int Id { get; set; }
        public string Sku { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public string CategoryName { get; set; } = string.Empty;
        public string BaseUnitCode { get; set; } = string.Empty;
        public DateTime? CreatedAt { get; set; }
    }
}
