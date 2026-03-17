namespace warehouseManagement.DTOs.DashboardManager
{
    public class RecentTransactionDto
    {
        public string Type { get; set; } = string.Empty;   // Inbound / Outbound / Transfer
        public int RefId { get; set; }
        public string RefNo { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public DateTime? CreatedAt { get; set; }
    }
}
