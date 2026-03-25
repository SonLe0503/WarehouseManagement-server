using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using warehouseManagement.DTOs;
using warehouseManagement.Models;

namespace warehouseManagement.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class InventoriesController : Controller
    {
        private readonly WmsContext _context;
        private readonly IMapper _mapper;
        public InventoriesController (WmsContext context , IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

            var user = await _context.Users
        .FirstOrDefaultAsync(x => x.Id == userId);

            if (user == null)
                return NotFound();

            var inventories = await _context.Inventories
        .Include(x => x.Product)
         .ThenInclude(p => p.BaseUnit)
        .Include(x => x.Warehouse)
        .Where(x => x.WarehouseId == user.WarehouseId)
        .OrderByDescending(x => x.UpdatedAt)
        .ToListAsync();

            var result = _mapper.Map<List<InventoryViewDto>>(inventories);
            return Ok(result);
            
        }

        [HttpGet("{id}")]

        public async Task<IActionResult> GetById(int id)
        {
            var inventory = await _context.Inventories
                .Include(x => x.Product)
                .Include(x => x.Warehouse)
                .FirstOrDefaultAsync(x  => x.Id == id);

            if (inventory == null)
                return NotFound("Inventory not found");

            var result = _mapper.Map<InventoryViewDto>(inventory);
            return Ok(result);
        }

        [HttpGet("bins")]
        public async Task<IActionResult> GetBinsByWarehouse([FromQuery] int warehouseId)
        {
            var bins = await _context.Inventories
                .Where(x => x.WarehouseId == warehouseId && x.StoragePosition != null)
                .Select(x => x.StoragePosition!)
                .Distinct()
                .OrderBy(x => x)
                .ToListAsync();

            return Ok(bins);
        }
    }
}
