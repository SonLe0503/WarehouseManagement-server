using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using warehouseManagement.Models;

namespace warehouseManagement.Controllers
{
    // ─── DTOs ───────────────────────────────────────────────────────────────────

    public class SalesOrderItemDto
    {
        public int Id { get; set; }
        public int ProductId { get; set; }
        public string? ProductName { get; set; }
        public decimal Quantity { get; set; }
    }

    public class SalesOrderDto
    {
        public int Id { get; set; }
        public string? OrderNo { get; set; }
        public string? CustomerName { get; set; }
        public string? Status { get; set; }
        public string? Note { get; set; }
        public int CreatedBy { get; set; }
        public DateTime? CreatedAt { get; set; }
        public List<SalesOrderItemDto> Items { get; set; } = new();
    }

    public class SalesOrderItemRequest
    {
        public int? Id { get; set; }          // null = tạo mới, có giá trị = update
        public int ProductId { get; set; }
        public decimal Quantity { get; set; }
    }

    public class SalesOrderRequest
    {
        public string? OrderNo { get; set; }
        public string? CustomerName { get; set; }
        public string? Status { get; set; }
        public string? Note { get; set; }
        public int CreatedBy { get; set; }
        public List<SalesOrderItemRequest> Items { get; set; } = new();
    }

    // ─── Controller ─────────────────────────────────────────────────────────────

    [Route("api/[controller]")]
    [ApiController]
    public class SalesOrdersController : ControllerBase
    {
        private readonly WmsContext _context;

        public SalesOrdersController(WmsContext context)
        {
            _context = context;
        }

        // ── Helpers ──────────────────────────────────────────────────────────────

        private static SalesOrderDto ToDto(SalesOrder o) => new()
        {
            Id = o.Id,
            OrderNo = o.OrderNo,
            CustomerName = o.CustomerName,
            Status = o.Status,
            Note = o.Note,
            CreatedBy = o.CreatedBy,
            CreatedAt = o.CreatedAt,
            Items = o.Items?.Select(i => new SalesOrderItemDto
            {
                Id = i.Id,
                ProductId = i.ProductId,
                ProductName = i.Product?.Name,
                Quantity = i.Quantity
            }).ToList() ?? new()
        };

        private async Task<string?> ValidateItems(List<SalesOrderItemRequest> items)
        {
            if (items == null || items.Count == 0)
                return "Order phải có ít nhất 1 sản phẩm.";

            foreach (var item in items)
            {
                if (item.Quantity <= 0)
                    return $"Số lượng của ProductId {item.ProductId} phải lớn hơn 0.";

                var exists = await _context.Products.AnyAsync(p => p.Id == item.ProductId);
                if (!exists)
                    return $"Sản phẩm với Id {item.ProductId} không tồn tại.";
            }

            return null; // hợp lệ
        }

        // ── GET all ──────────────────────────────────────────────────────────────

        [HttpGet]
        public async Task<ActionResult<IEnumerable<SalesOrderDto>>> GetSalesOrders()
        {
            var orders = await _context.SalesOrders
                .Include(o => o.Items)
                    .ThenInclude(i => i.Product)
                .ToListAsync();

            return Ok(orders.Select(ToDto));
        }

        // ── GET by id ────────────────────────────────────────────────────────────

        [HttpGet("{id}")]
        public async Task<ActionResult<SalesOrderDto>> GetSalesOrder(int id)
        {
            var order = await _context.SalesOrders
                .Include(o => o.Items)
                    .ThenInclude(i => i.Product)
                .FirstOrDefaultAsync(o => o.Id == id);

            if (order == null) return NotFound();

            return Ok(ToDto(order));
        }

        // ── POST ─────────────────────────────────────────────────────────────────

        [HttpPost]
        public async Task<ActionResult<SalesOrderDto>> PostSalesOrder(SalesOrderRequest req)
        {
            var error = await ValidateItems(req.Items);
            if (error != null) return BadRequest(error);

            var order = new SalesOrder
            {
                OrderNo = req.OrderNo,
                CustomerName = req.CustomerName,
                Status = req.Status,
                Note = req.Note,
                CreatedBy = req.CreatedBy,
                CreatedAt = DateTime.UtcNow,
                Items = req.Items.Select(i => new SalesOrderItem
                {
                    ProductId = i.ProductId,
                    Quantity = i.Quantity
                }).ToList()
            };

            _context.SalesOrders.Add(order);
            await _context.SaveChangesAsync();

            // Load lại để có Product name
            await _context.Entry(order)
                .Collection(o => o.Items)
                .Query()
                .Include(i => i.Product)
                .LoadAsync();

            return CreatedAtAction(nameof(GetSalesOrder), new { id = order.Id }, ToDto(order));
        }

        // ── PUT ──────────────────────────────────────────────────────────────────

        [HttpPut("{id}")]
        public async Task<ActionResult<SalesOrderDto>> PutSalesOrder(int id, SalesOrderRequest req)
        {
            var order = await _context.SalesOrders
                .Include(o => o.Items)
                .FirstOrDefaultAsync(o => o.Id == id);

            if (order == null) return NotFound();

            var error = await ValidateItems(req.Items);
            if (error != null) return BadRequest(error);

            // Cập nhật header
            order.OrderNo = req.OrderNo;
            order.CustomerName = req.CustomerName;
            order.Status = req.Status;
            order.Note = req.Note;

            // ── Xử lý Items ──────────────────────────────────────────────────────
            var requestedIds = req.Items
                .Where(i => i.Id.HasValue)
                .Select(i => i.Id!.Value)
                .ToHashSet();

            // Xóa những item không còn trong request
            var toDelete = order.Items
                .Where(i => !requestedIds.Contains(i.Id))
                .ToList();
            _context.SalesOrderItems.RemoveRange(toDelete);

            foreach (var itemReq in req.Items)
            {
                if (itemReq.Id.HasValue)
                {
                    // Update item đã tồn tại
                    var existing = order.Items.FirstOrDefault(i => i.Id == itemReq.Id.Value);
                    if (existing != null)
                    {
                        existing.ProductId = itemReq.ProductId;
                        existing.Quantity = itemReq.Quantity;
                    }
                }
                else
                {
                    // Thêm item mới
                    order.Items.Add(new SalesOrderItem
                    {
                        ProductId = itemReq.ProductId,
                        Quantity = itemReq.Quantity
                    });
                }
            }

            await _context.SaveChangesAsync();

            // Load lại Product name
            await _context.Entry(order)
                .Collection(o => o.Items)
                .Query()
                .Include(i => i.Product)
                .LoadAsync();

            return Ok(ToDto(order));
        }

        // ── DELETE ───────────────────────────────────────────────────────────────

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteSalesOrder(int id)
        {
            var order = await _context.SalesOrders
                .Include(o => o.Items)
                .FirstOrDefaultAsync(o => o.Id == id);

            if (order == null) return NotFound();

            _context.SalesOrderItems.RemoveRange(order.Items);
            _context.SalesOrders.Remove(order);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}