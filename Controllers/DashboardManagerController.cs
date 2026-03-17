using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using warehouseManagement.DTOs.DashboardManager;
using warehouseManagement.Models;

namespace warehouseManagement.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class DashboardManagerController : Controller
    {
        private readonly WmsContext _context;
        public DashboardManagerController(WmsContext context)
        {
            _context = context;
        }
        private int GetCurrentUserId()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(userIdClaim))
                throw new UnauthorizedAccessException("UserId claim not found.");

            return int.Parse(userIdClaim);
        }

        private async Task<int> GetCurrentWarehouseIdAsync()
        {
            var userId = GetCurrentUserId();

            var user = await _context.Users
                .FirstOrDefaultAsync(x => x.Id == userId);

            if (user == null)
                throw new Exception("User not found.");

            if (user.WarehouseId == null)
                throw new Exception("Manager is not assigned to any warehouse.");

            return user.WarehouseId.Value;
        }

        // 1. Summary
        [HttpGet("summary")]
        public async Task<IActionResult> GetSummary([FromQuery] decimal lowStockThreshold = 10)
        {
            var warehouseId = await GetCurrentWarehouseIdAsync();

            var inventoryByProduct = await _context.Inventories
                .Where(i => i.WarehouseId == warehouseId)
                .GroupBy(i => i.ProductId)
                .Select(g => new
                {
                    ProductId = g.Key,
                    Quantity = g.Sum(x => x.Quantity)
                })
                .ToListAsync();

            int totalProductsInStock = inventoryByProduct.Count(x => x.Quantity > 0);
            decimal totalQuantityInWarehouse = inventoryByProduct.Sum(x => x.Quantity);
            int lowStockItems = inventoryByProduct.Count(x => x.Quantity < lowStockThreshold);

            var today = DateTime.Today;
            var tomorrow = today.AddDays(1);

            var pendingInbound = await _context.InboundRequests
                .CountAsync(x => x.WarehouseId == warehouseId && x.Status == "Pending");

            var pendingOutbound = await _context.OutboundRequests
                .CountAsync(x => x.WarehouseId == warehouseId && x.Status == "Pending");

            var pendingTransferIn = await _context.StockTransferRequests
                .CountAsync(x => x.ToWarehouseId == warehouseId && x.Status == "Pending");

            var pendingTransferOut = await _context.StockTransferRequests
                .CountAsync(x => x.FromWarehouseId == warehouseId && x.Status == "Pending");

            var todayInbound = await _context.InboundItems
                .Where(ii => ii.InboundRequest.WarehouseId == warehouseId
                             && ii.InboundRequest.CreatedAt >= today
                             && ii.InboundRequest.CreatedAt < tomorrow)
                .SumAsync(ii => (decimal?)(ii.ReceivedQuantity ?? ii.Quantity)) ?? 0;

            var todayOutbound = await _context.OutboundItems
                .Where(oi => oi.OutboundRequest.WarehouseId == warehouseId
                             && oi.OutboundRequest.CreatedAt >= today
                             && oi.OutboundRequest.CreatedAt < tomorrow)
                .SumAsync(oi => (decimal?)(oi.PickedQuantity ?? oi.Quantity)) ?? 0;

            var result = new DashboardSummaryDto
            {
                TotalProductsInStock = totalProductsInStock,
                TotalQuantityInWarehouse = totalQuantityInWarehouse,
                LowStockItems = lowStockItems,
                PendingRequests = pendingInbound + pendingOutbound + pendingTransferIn + pendingTransferOut,
                TodayInbound = todayInbound,
                TodayOutbound = todayOutbound
            };

            return Ok(result);
        }

        // 2. Inbound vs Outbound Chart
        [HttpGet("inbound-outbound-chart")]
        public async Task<IActionResult> GetInboundOutboundChart([FromQuery] string period = "week")
        {
            var warehouseId = await GetCurrentWarehouseIdAsync();

            int days = period.ToLower() == "month" ? 30 : 7;
            var fromDate = DateTime.Today.AddDays(-(days - 1));
            var toDate = DateTime.Today.AddDays(1);

            var inboundData = await _context.InboundItems
                .Where(ii => ii.InboundRequest.WarehouseId == warehouseId
                             && ii.InboundRequest.CreatedAt >= fromDate
                             && ii.InboundRequest.CreatedAt < toDate)
                .GroupBy(ii => ii.InboundRequest.CreatedAt!.Value.Date)
                .Select(g => new
                {
                    Date = g.Key,
                    Quantity = g.Sum(x => x.ReceivedQuantity ?? x.Quantity)
                })
                .ToListAsync();

            var outboundData = await _context.OutboundItems
                .Where(oi => oi.OutboundRequest.WarehouseId == warehouseId
                             && oi.OutboundRequest.CreatedAt >= fromDate
                             && oi.OutboundRequest.CreatedAt < toDate)
                .GroupBy(oi => oi.OutboundRequest.CreatedAt!.Value.Date)
                .Select(g => new
                {
                    Date = g.Key,
                    Quantity = g.Sum(x => x.PickedQuantity ?? x.Quantity)
                })
                .ToListAsync();

            var result = new List<InboundOutboundChartPointDto>();

            for (int i = 0; i < days; i++)
            {
                var date = fromDate.AddDays(i).Date;

                var purchases = inboundData
                    .Where(x => x.Date == date)
                    .Select(x => x.Quantity)
                    .FirstOrDefault();

                var sales = outboundData
                    .Where(x => x.Date == date)
                    .Select(x => x.Quantity)
                    .FirstOrDefault();

                result.Add(new InboundOutboundChartPointDto
                {
                    Label = date.ToString("yyyy-MM-dd"),
                    Purchases = purchases,
                    Sales = sales
                });
            }

            return Ok(result);
        }

        // 3. Low Stock
        [HttpGet("low-stock")]
        public async Task<IActionResult> GetLowStock([FromQuery] decimal threshold = 10, [FromQuery] int take = 10)
        {
            var warehouseId = await GetCurrentWarehouseIdAsync();

            var result = await _context.Inventories
                .Where(i => i.WarehouseId == warehouseId)
                .GroupBy(i => new { i.ProductId, i.Product.Sku, i.Product.Name })
                .Select(g => new LowStockDto
                {
                    ProductId = g.Key.ProductId,
                    SKU = g.Key.Sku,
                    ProductName = g.Key.Name,
                    CurrentQuantity = g.Sum(x => x.Quantity),
                    Status = g.Sum(x => x.Quantity) <= 0 ? "Out of Stock" : "Low Stock"
                })
                .Where(x => x.CurrentQuantity < threshold)
                .OrderBy(x => x.CurrentQuantity)
                .Take(take)
                .ToListAsync();

            return Ok(result);
        }

        // 4. Pending Requests
        [HttpGet("pending-requests")]
        public async Task<IActionResult> GetPendingRequests()
        {
            var warehouseId = await GetCurrentWarehouseIdAsync();

            var pendingInbound = await _context.InboundRequests
                .CountAsync(x => x.WarehouseId == warehouseId && x.Status == "Pending");

            var pendingOutbound = await _context.OutboundRequests
                .CountAsync(x => x.WarehouseId == warehouseId && x.Status == "Pending");

            var pendingTransferIn = await _context.StockTransferRequests
                .CountAsync(x => x.ToWarehouseId == warehouseId && x.Status == "Pending");

            var pendingTransferOut = await _context.StockTransferRequests
                .CountAsync(x => x.FromWarehouseId == warehouseId && x.Status == "Pending");

            var result = new PendingRequestsDto
            {
                PendingInboundRequests = pendingInbound,
                PendingOutboundRequests = pendingOutbound,
                PendingTransferInRequests = pendingTransferIn,
                PendingTransferOutRequests = pendingTransferOut,
                TotalPendingRequests = pendingInbound + pendingOutbound + pendingTransferIn + pendingTransferOut
            };

            return Ok(result);
        }

        // 5. Recent Transactions
        [HttpGet("recent-transactions")]
        public async Task<IActionResult> GetRecentTransactions([FromQuery] int take = 10)
        {
            var warehouseId = await GetCurrentWarehouseIdAsync();

            var inbound = await _context.InboundRequests
                .Where(x => x.WarehouseId == warehouseId)
                .Select(x => new RecentTransactionDto
                {
                    Type = "Inbound",
                    RefId = x.Id,
                    RefNo = x.RequestNo ?? ("INB-" + x.Id),
                    Status = x.Status,
                    CreatedAt = x.CreatedAt
                })
                .ToListAsync();

            var outbound = await _context.OutboundRequests
                .Where(x => x.WarehouseId == warehouseId)
                .Select(x => new RecentTransactionDto
                {
                    Type = "Outbound",
                    RefId = x.Id,
                    RefNo = x.RequestNo ?? ("OUT-" + x.Id),
                    Status = x.Status,
                    CreatedAt = x.CreatedAt
                })
                .ToListAsync();

            var transferIn = await _context.StockTransferRequests
                .Where(x => x.ToWarehouseId == warehouseId)
                .Select(x => new RecentTransactionDto
                {
                    Type = "Transfer In",
                    RefId = x.Id,
                    RefNo = x.TransferNo ?? ("TRF-IN-" + x.Id),
                    Status = x.Status,
                    CreatedAt = x.CreatedAt
                })
                .ToListAsync();

            var transferOut = await _context.StockTransferRequests
                .Where(x => x.FromWarehouseId == warehouseId)
                .Select(x => new RecentTransactionDto
                {
                    Type = "Transfer Out",
                    RefId = x.Id,
                    RefNo = x.TransferNo ?? ("TRF-OUT-" + x.Id),
                    Status = x.Status,
                    CreatedAt = x.CreatedAt
                })
                .ToListAsync();

            var result = inbound
                .Concat(outbound)
                .Concat(transferIn)
                .Concat(transferOut)
                .OrderByDescending(x => x.CreatedAt)
                .Take(take)
                .ToList();

            return Ok(result);
        }
    }
}
