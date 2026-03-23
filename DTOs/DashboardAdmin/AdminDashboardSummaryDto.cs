namespace warehouseManagement.DTOs.DashboardAdmin
{
    public class AdminDashboardSummaryDto
    {
        public int TotalUsers { get; set; }
        public int ActiveUsers { get; set; }
        public int InactiveUsers { get; set; }
        public int TotalWarehouses { get; set; }
        public int ActiveWarehouses { get; set; }
        public int InactiveWarehouses { get; set; }
        public int WarehousesWithoutManager { get; set; }
        public int TotalProducts { get; set; }
        public int ActiveProducts { get; set; }
        public int InactiveProducts { get; set; }
        public int TotalCategories { get; set; }
        public int RootCategories { get; set; }
        public int TotalUnits { get; set; }
        public int BaseUnits { get; set; }
    }
}
