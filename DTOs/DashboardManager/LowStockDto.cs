namespace warehouseManagement.DTOs.DashboardManager
{
    public class LowStockDto
    {
        public int ProductId { get; set; }
        public string SKU { get; set; } = string.Empty;
        public string ProductName { get; set; } = string.Empty;
        public decimal CurrentQuantity { get; set; }
        public string Status { get; set; } = string.Empty;
    }
}
