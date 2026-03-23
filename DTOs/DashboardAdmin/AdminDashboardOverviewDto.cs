namespace warehouseManagement.DTOs.DashboardAdmin
{
    public class AdminDashboardOverviewDto
    {
        public AdminDashboardSummaryDto Summary { get; set; } = new();
        public List<RoleDistributionDto> RoleDistribution { get; set; } = new();
        public List<RecentUserDto> RecentUsers { get; set; } = new();
        public List<RecentProductDto> RecentProducts { get; set; } = new();
        public List<WarehouseOverviewDto> WarehouseOverview { get; set; } = new();
    }
}
