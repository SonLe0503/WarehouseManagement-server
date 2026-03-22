using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using warehouseManagement.DTOs.DashboardAdmin;
using warehouseManagement.Models;

namespace warehouseManagement.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "ADMIN")]
    public class DashboardAdminController : Controller
    {
        private readonly WmsContext _context;

        public DashboardAdminController(WmsContext context)
        {
            _context = context;
        }

        [HttpGet("overview")]
        public async Task<IActionResult> GetOverview()
        {
            var users = await _context.Users
                .Include(u => u.Roles)
                .Include(u => u.Warehouse)
                .ToListAsync();

            var warehouses = await _context.Warehouses.ToListAsync();
            var products = await _context.Products
                .Include(p => p.Category)
                .Include(p => p.BaseUnit)
                .ToListAsync();
            var categories = await _context.Categories.ToListAsync();
            var units = await _context.Units.ToListAsync();
            var roles = await _context.Roles
                .Include(r => r.Users)
                .OrderBy(r => r.Id)
                .ToListAsync();

            var warehouseManagerMap = users
                .Where(u => u.WarehouseId.HasValue && u.Roles.Any(r => r.Name == "MANAGE"))
                .GroupBy(u => u.WarehouseId!.Value)
                .ToDictionary(g => g.Key, g => g.Count());

            var summary = new AdminDashboardSummaryDto
            {
                TotalUsers = users.Count,
                ActiveUsers = users.Count(u => string.Equals(u.Status, "Active", StringComparison.OrdinalIgnoreCase)),
                InactiveUsers = users.Count(u => !string.Equals(u.Status, "Active", StringComparison.OrdinalIgnoreCase)),
                TotalWarehouses = warehouses.Count,
                ActiveWarehouses = warehouses.Count(w => string.Equals(w.Status, "Active", StringComparison.OrdinalIgnoreCase)),
                InactiveWarehouses = warehouses.Count(w => !string.Equals(w.Status, "Active", StringComparison.OrdinalIgnoreCase)),
                WarehousesWithoutManager = warehouses.Count(w => !warehouseManagerMap.ContainsKey(w.Id)),
                TotalProducts = products.Count,
                ActiveProducts = products.Count(p => string.Equals(p.Status, "ACTIVE", StringComparison.OrdinalIgnoreCase)),
                InactiveProducts = products.Count(p => !string.Equals(p.Status, "ACTIVE", StringComparison.OrdinalIgnoreCase)),
                TotalCategories = categories.Count,
                RootCategories = categories.Count(c => c.ParentId == null),
                TotalUnits = units.Count,
                BaseUnits = units.Count(u => u.IsBaseUnit)
            };

            var roleDistribution = roles
                .Select(role => new RoleDistributionDto
                {
                    RoleName = role.Name,
                    UserCount = role.Users.Count
                })
                .ToList();

            var recentUsers = users
                .OrderByDescending(u => u.CreatedAt)
                .Take(6)
                .Select(u => new RecentUserDto
                {
                    Id = u.Id,
                    Username = u.Username,
                    Email = u.Email ?? string.Empty,
                    Status = u.Status ?? string.Empty,
                    Roles = u.Roles.Select(r => r.Name).ToList(),
                    WarehouseName = u.Warehouse?.Name,
                    CreatedAt = u.CreatedAt
                })
                .ToList();

            var recentProducts = products
                .OrderByDescending(p => p.CreatedAt)
                .Take(6)
                .Select(p => new RecentProductDto
                {
                    Id = p.Id,
                    Sku = p.Sku,
                    Name = p.Name,
                    Status = p.Status ?? string.Empty,
                    CategoryName = p.Category?.Name ?? string.Empty,
                    BaseUnitCode = p.BaseUnit?.Code ?? string.Empty,
                    CreatedAt = p.CreatedAt
                })
                .ToList();

            var warehouseOverview = warehouses
                .OrderBy(w => w.Name)
                .Select(w => new WarehouseOverviewDto
                {
                    WarehouseId = w.Id,
                    Code = w.Code,
                    Name = w.Name,
                    Status = w.Status ?? string.Empty,
                    UserCount = users.Count(u => u.WarehouseId == w.Id),
                    ManagerCount = warehouseManagerMap.TryGetValue(w.Id, out var managerCount) ? managerCount : 0
                })
                .ToList();

            var result = new AdminDashboardOverviewDto
            {
                Summary = summary,
                RoleDistribution = roleDistribution,
                RecentUsers = recentUsers,
                RecentProducts = recentProducts,
                WarehouseOverview = warehouseOverview
            };

            return Ok(result);
        }
    }
}
