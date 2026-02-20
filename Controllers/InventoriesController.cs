using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using warehouseManagement.Models;

namespace warehouseManagement.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class InventoriesController : Controller
    {
        private readonly WmsContext _context;
        public InventoriesController (WmsContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var inventories =  await _context.Inventories
                .Include(x => x.Product)
                .Include(x => x.Warehouse)
                .OrderByDescending(x => x.UpdatedAt)
                .ToListAsync();
            return Ok(inventories);
        }

        [HttpGet("{id}")]

        public async Task<IActionResult> GetById(int id)
        {
            var inventory = await _context.Inventories
                .Include(x => x.Product)
                .Include(x => x.Warehouse)
                .FirstOrDefaultAsync(x  => x.Id == id);
            if (inventory == null)
            {
                return NotFound("Inventory not found");
            }

            return Ok(inventory);
        }
    }
}
