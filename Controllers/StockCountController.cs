using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using warehouseManagement.DTOs.Sessions;
using warehouseManagement.Models;

namespace warehouseManagement.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class StockCountController : Controller
    {
        private readonly WmsContext _context;
        private readonly IMapper _mapper;

        public StockCountController(WmsContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        private int GetCurrentUserId()
        {
            var claim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (claim == null) throw new UnauthorizedAccessException();
            return int.Parse(claim);
        }

        [HttpPost]
        public async Task<IActionResult> CreateSession(CreateStockCountSessionDTO dto)
        {
            var userId = GetCurrentUserId();

            var user = await _context.Users.FirstOrDefaultAsync(x => x.Id == userId);
            if (user == null) return NotFound();

            // thêm kiểm tra vì sao ( vì ở tockCountSession là "int WarehouseId" còn  ở user là " int ? warehouseId")
            if (user.WarehouseId == null)
                return BadRequest("Tài khoản của bạn chưa được gán kho hàng. Vui lòng liên hệ Admin.");


            var session = new StockCountSession
            {
                CountNo = "SC" + DateTime.Now.Ticks,
                WarehouseId = user.WarehouseId.Value,
                Note = dto.Note,
                Status = "Draft",
                CreatedBy = userId,
                CreatedAt = DateTime.UtcNow
            };

            _context.StockCountSessions.Add(session);
            await _context.SaveChangesAsync();

            return Ok(_mapper.Map<StockCountSessionDTO>(session));
        }

        [HttpPost("sessions/{id}/generate-items")]
        public async Task<IActionResult> GenerateItems(int id, [FromBody] GenerateStockCountItemsDTO dto)
        {
            var session = await _context.StockCountSessions.FindAsync(id);
            if (session == null) return NotFound("Phiên kiểm kê không tồn tại");

            if (session.Status != "Draft")
                return BadRequest("Phiên kiểm kê đã được khởi tạo trước đó");

            var selectedBins = await _context.Bins
                .Where(b => dto.BinIds.Contains(b.Id) && b.WarehouseId == session.WarehouseId)
                .ToListAsync();

            if (!selectedBins.Any())
                return BadRequest("Không tìm thấy bin hợp lệ trong kho này");

            var binCodes = selectedBins.Select(b => b.Code).ToList();

            var inventories = await _context.Inventories
                .Where(x => x.WarehouseId == session.WarehouseId
                         && binCodes.Contains(x.StoragePosition))
                .ToListAsync();

            if (!inventories.Any())
                return BadRequest("Các bin được chọn không có hàng hóa nào");

            var items = inventories.Select(i => new StockCountItem
            {
                StockCountSessionId = id,
                ProductId = i.ProductId,
                StoragePosition = i.StoragePosition,
                SystemQuantity = i.Quantity
            }).ToList();

            _context.StockCountItems.AddRange(items);
            session.Status = "Counting";

            await _context.SaveChangesAsync();

            return Ok(new { message = "Khởi tạo thành công", itemCount = items.Count });
        }

        [HttpGet("sessions")]
        public async Task<IActionResult> GetSessions()
        {
            var userId = GetCurrentUserId();

            var user = await _context.Users.FirstOrDefaultAsync(x => x.Id == userId);
            if (user == null) return NotFound();

            var sessions = await _context.StockCountSessions
                .Where(x => x.WarehouseId == user.WarehouseId)
                .OrderByDescending(x => x.CreatedAt)
                .ToListAsync();

            return Ok(_mapper.Map<List<StockCountSessionDTO>>(sessions));
        }

        [HttpGet("sessions/{id}/items")]
        public async Task<IActionResult> GetItems(int id)
        {
            var items = await _context.StockCountItems
                .Where(x => x.StockCountSessionId == id)
                .Include(x => x.Product)
                .ThenInclude(p => p.BaseUnit)
                .ToListAsync();

            return Ok(_mapper.Map<List<StockCountItemDTO>>(items));
        }

        [HttpPut("items/{id}")]
        public async Task<IActionResult> UpdateActualQuantity(int id, UpdateActualQuantityDTO dto)
        {
            var item = await _context.StockCountItems.FindAsync(id);
            if (item == null) return NotFound();

            item.ActualQuantity = dto.ActualQuantity;
            item.Difference = dto.ActualQuantity - item.SystemQuantity;
            item.Note = dto.Note;

            await _context.SaveChangesAsync();
            return Ok();
        }

        [HttpPost("sessions/{id}/approve")]
        public async Task<IActionResult> ApproveSession(int id)
        {
            var userId = GetCurrentUserId();

            var session = await _context.StockCountSessions
                .Include(x => x.Items)
                .FirstOrDefaultAsync(x => x.Id == id);

            if (session == null) return NotFound();

            foreach (var item in session.Items)
            {
                if (item.ActualQuantity == null) continue;

                var inventory = await _context.Inventories
                    .FirstOrDefaultAsync(x =>
                        x.ProductId == item.ProductId &&
                        x.WarehouseId == session.WarehouseId &&
                        x.StoragePosition == item.StoragePosition);

                if (inventory != null)
                    inventory.Quantity = item.ActualQuantity.Value;
            }

            session.Status = "Approved";
            session.ApprovedBy = userId;  
            session.ApprovedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return Ok();
        }
    }
}