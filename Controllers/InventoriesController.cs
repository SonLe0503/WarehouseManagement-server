using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
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
            var inventories =  await _context.Inventories
                .Include(x => x.Product)
                .Include(x => x.Warehouse)
                .Include(x => x.Unit)
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
    }
}
