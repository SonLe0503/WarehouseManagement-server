namespace warehouseManagement.DTOs.DashboardManager
{
    public class DashboardSummaryDto
    {
        public int TotalProductsInStock { get; set; }      // số product có tồn > 0
        public decimal TotalQuantityInWarehouse { get; set; } // tổng số lượng tồn
        public int LowStockItems { get; set; }             // tồn < threshold
        public int PendingRequests { get; set; }           // tổng pending
        public decimal TodayInbound { get; set; }          // tổng nhập hôm nay
        public decimal TodayOutbound { get; set; }
    }
}
