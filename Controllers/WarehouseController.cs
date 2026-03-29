using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using warehouseManagement.DTOs;
using warehouseManagement.Models;

namespace warehouseManagement.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class WarehouseController : Controller
    {
        private readonly WmsContext _context;
        public WarehouseController(WmsContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll() 
        {
            var warehouses = await _context.Warehouses
                .Select(w => new WarehouseDTO
                {
                    Id = w.Id,
                    Code = w.Code,
                    Name = w.Name,
                    Address = w.Address,
                    Status = w.Status,
                    CreatedAt = w.CreatedAt
                })
                .ToListAsync();
            return Ok(warehouses);
        }

        [HttpGet("{id}")]

        public async Task<IActionResult> GetById(int id)
        {
            var warehouse = await _context.Warehouses
                .Where(w => w.Id == id)
                .Select(w => new WarehouseDTO
                {
                    Id = w.Id,
                    Code = w.Code,
                    Name = w.Name,
                    Address = w.Address,
                    Status = w.Status,
                    CreatedAt = w.CreatedAt
                })
                .FirstOrDefaultAsync();
            if (warehouse == null)
            {
                return NotFound("Warehouse not found");
            }
            return Ok(warehouse);
        }

        [HttpPost]
        public async Task<IActionResult> Create(CreateWarehouseDTO dto)
        {
            if (await _context.Warehouses.AnyAsync(w => w.Code == dto.Code))
            {
                return BadRequest("Warehouse code already exists.");
            }
            var warehouse = new Warehouse
            {
                Code = dto.Code,
                Name = dto.Name,
                Address = dto.Address,
                Status = "Active",
                CreatedAt = DateTime.UtcNow
            };

            _context.Warehouses.Add(warehouse);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Warehouse created successfully"});
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id,  WarehouseDTO dto)
        {
            var warehouse  = await _context.Warehouses.FindAsync(id);
            if (warehouse == null)
                return NotFound("Warehouse not found");

            //if (await _context.Warehouses
            //    .AnyAsync(w => w.Code == dto.Code && w.Id != dto.Id))
            //{
            //    return BadRequest("Warehouse code already exists.");
            //}
            warehouse.Code = dto.Code;
            warehouse.Name = dto.Name;
            warehouse.Address = dto.Address;

            await _context.SaveChangesAsync();
            return Ok(new { message = "Warehouse updated successfully" });
        }

        [HttpPatch("{id}/activate")]
        public async Task<IActionResult> Activate(int id)
        {
            var warehouse = await _context.Warehouses.FindAsync(id);
            if (warehouse == null)
                return NotFound("Warehouse not found");

            if (warehouse.Status == "Active")
                return BadRequest("Warehouse already active");

            warehouse.Status = "Active";

            await _context.SaveChangesAsync();

            return Ok(new { message = "Warehouse activated successfully" });
        }


        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var warehouse = await _context.Warehouses.FindAsync(id);
            if (warehouse == null)
                return NotFound("Warehouse not found");

            var hasStock = await _context.Inventories
                .AnyAsync(i => i.WarehouseId == id && i.Quantity > 0);

            if (hasStock)
                return BadRequest("Cannot deactivate warehouse because it still has inventory.");

            if (warehouse.Status == "Inactive")
                return BadRequest("Warehouse already inactive");

            warehouse.Status = "Inactive";
            await _context.SaveChangesAsync();
            return Ok(new { message = "Warehouse deactived successfully" });
        }
    }
}
