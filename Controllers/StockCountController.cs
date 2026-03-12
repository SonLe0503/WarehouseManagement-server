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
       
    }
}
