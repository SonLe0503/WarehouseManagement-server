namespace warehouseManagement.DTOs.DashboardManager
{
    public class InboundOutboundChartPointDto
    {
        public string Label { get; set; } = string.Empty; // yyyy-MM-dd
        public decimal Purchases { get; set; }            // nhập
        public decimal Sales { get; set; }
    }
}
