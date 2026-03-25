using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using warehouseManagement.DTOs.Sessions;
using warehouseManagement.Models;

namespace warehouseManagement.Controllers
{
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

        [HttpPost]
        public async Task<IActionResult> CreateSession(CreateStockCountSessionDTO dto) 
        {
            var session = new StockCountSession
            {
                CountNo = "SC" + DateTime.Now.Ticks,
                WarehouseId = dto.WarehouseId,
                Note = dto.Note,
                Status = "Draft",
                CreatedBy = 1,
                CreatedAt = DateTime.UtcNow
            };
            _context.StockCountSessions.Add(session);
            await _context.SaveChangesAsync();

            return Ok(_mapper.Map<StockCountSessionDTO>(session));
        }
        [HttpPost("sessions/{id}/generate-items")]
        public async Task<IActionResult> GenerateItems(int id)
        {
            var session = await _context.StockCountSessions.FindAsync(id);

            if (session == null)
                return NotFound();

            var inventories = await _context.Inventories
                .Where(x => x.WarehouseId == session.WarehouseId)
                .ToListAsync();

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

            return Ok();
        }

        // 3️⃣ Get session list
        [HttpGet("sessions")]
        public async Task<IActionResult> GetSessions()
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

            var user = await _context.Users
        .FirstOrDefaultAsync(x => x.Id == userId);

            if (user == null)
                return NotFound();
            var sessions = await _context.StockCountSessions
                .Where(x => x.WarehouseId == user.WarehouseId)
                .ToListAsync();

            return Ok(_mapper.Map<List<StockCountSessionDTO>>(sessions));
        }

        // 4️⃣ Get items in session
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

        // 5️⃣ Update actual quantity
        [HttpPut("items/{id}")]
        public async Task<IActionResult> UpdateActualQuantity(int id, UpdateActualQuantityDTO dto)
        {
            var item = await _context.StockCountItems.FindAsync(id);

            if (item == null)
                return NotFound();

            item.ActualQuantity = dto.ActualQuantity;
            item.Difference = dto.ActualQuantity - item.SystemQuantity;
            item.ReasonId = dto.ReasonId;
            item.Note = dto.Note;

            await _context.SaveChangesAsync();

            return Ok();
        }

        // 6️⃣ Approve session
        [HttpPost("sessions/{id}/approve")]
        public async Task<IActionResult> ApproveSession(int id)
        {
            var session = await _context.StockCountSessions
                .Include(x => x.Items)
                .FirstOrDefaultAsync(x => x.Id == id);

            if (session == null)
                return NotFound();

            foreach (var item in session.Items)
            {
                if (item.ActualQuantity == null)
                    continue;

                var inventory = await _context.Inventories
                    .FirstOrDefaultAsync(x =>
                        x.ProductId == item.ProductId &&
                        x.WarehouseId == session.WarehouseId &&
                        x.StoragePosition == item.StoragePosition);

                if (inventory != null)
                {
                    inventory.Quantity = item.ActualQuantity.Value;
                }
            }

            session.Status = "Approved";
            session.ApprovedBy = 1;
            session.ApprovedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return Ok();
        }

    }
}
