using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
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
            var sessions = await _context.StockCountSessions.ToListAsync();

            return Ok(_mapper.Map<List<StockCountSessionDTO>>(sessions));
        }

        // 4️⃣ Get items in session
        [HttpGet("sessions/{id}/items")]
        public async Task<IActionResult> GetItems(int id)
        {
            var items = await _context.StockCountItems
                .Where(x => x.StockCountSessionId == id)
                .Include(x => x.Product)
                .ToListAsync();

            return Ok(_mapper.Map<List<StockCountItemDTO>>(items));
        }

    }
}
