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

            var result = inventories.Select(x => new InventoryViewDto
            {
                Id = x.Id,
                ProductId = x.ProductId,
                ProductName = x.Product?.Name,
                Sku = x.Product?.Sku,
                WarehouseId = x.WarehouseId,
                WarehouseName = x.Warehouse?.Name,
                WarehouseCode = x.Warehouse?.Code,
                Quantity = x.Quantity,
                StoragePosition = x.StoragePosition,
                UpdatedAt = x.UpdatedAt

            }).ToList();
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
            {
                return NotFound("Inventory not found");
            }
            var result = new InventoryViewDto
            {
                Id = inventory.Id,
                ProductId = inventory.ProductId,
                ProductName = inventory.Product?.Name,
                Sku = inventory.Product?.Sku,
                WarehouseId = inventory.WarehouseId,
                WarehouseName = inventory.Warehouse?.Name,
                WarehouseCode = inventory.Warehouse?.Code,
                Quantity = inventory.Quantity,
                StoragePosition = inventory.StoragePosition,
                UpdatedAt = inventory.UpdatedAt


            };

           

            return Ok(result);
        }
    }
}
