using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using warehouseManagement.DTOs;
using warehouseManagement.Models;

namespace warehouseManagement.Controllers
{
    [ApiController]
    [Authorize]
    [Route("api/warehouses")]
    public class WarehousesController : ControllerBase
    {
        private readonly WmsContext _context;

        public WarehousesController(WmsContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<WarehouseLookupDto>>> GetActiveWarehouses()
        {
            var warehouses = await _context.Warehouses
                .AsNoTracking()
                .Where(w => w.Status == "Active")
                .Select(w => new WarehouseLookupDto
                {
                    Id = w.Id,
                    Code = w.Code,
                    Name = w.Name
                })
                .ToListAsync();

            if (warehouses == null || !warehouses.Any())
            {
                return Ok(new List<WarehouseLookupDto>());
            }

            return Ok(warehouses);
        }

        [HttpGet("validate/{id}")]
        public async Task<IActionResult> ValidateWarehouse(int id)
        {
            var isValid = await _context.Warehouses
                .AnyAsync(w => w.Id == id && w.Status == "Active");

            if (!isValid)
            {
                return BadRequest("Kho không tồn tại hoặc không ở trạng thái hoạt động.");
            }

            return Ok(new { valid = true });
        }
    }
}