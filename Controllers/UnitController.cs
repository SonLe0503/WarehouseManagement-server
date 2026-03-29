using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using warehouseManagement.DTOs;
using warehouseManagement.Models;

namespace warehouseManagement.Controllers
{
    [ApiController]
    [Authorize]
    [Route("api/[controller]")]
    public class UnitController : Controller
    {
        private readonly WmsContext _context;
        public UnitController(WmsContext context) 
        { 
            _context = context;
        }
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var units = await _context.Units
                .OrderBy(u => u.Name)
                .ToListAsync();
            return Ok(units);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var unit = await _context.Units.FindAsync(id);
            if(unit == null)
            {
                return NotFound("Unit not found");
            }
            return Ok(unit);
        }

        [HttpPost]
        public async Task<IActionResult> Create(UnitDTO dto)
        {
            var exists = await _context.Units
                .AnyAsync(u => u.Code == dto.Code);
            if(exists)
            {
                return BadRequest("Unit code already exists");
            }
            var unit = new Unit
            {
                Code = dto.Code,
                Name = dto.Name,
                Description = dto.Description,
                IsBaseUnit = dto.IsBaseUnit,
                CreatedAt = DateTime.UtcNow
            };
            _context.Units.Add(unit);
            await _context.SaveChangesAsync();
            return Ok(unit);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, UnitDTO dto)
        {
            var unit = await _context.Units.FindAsync(id);
            if (unit == null)
                return NotFound("Unit not found");

            var exists = await _context.Units
                .AnyAsync(u => u.Code == dto.Code && u.Id != id);

            if (exists)
                return BadRequest("Unit code already exists");

            unit.Code = dto.Code;
            unit.Name = dto.Name;
            unit.Description = dto.Description;
            unit.IsBaseUnit = dto.IsBaseUnit;
            unit.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return Ok(unit);
        }
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var unit  = await _context.Units
                .Include(u => u.Products)
                .Include(u => u.UnitConversionBaseUnits)
                .Include(u => u.UnitConversionFromUnits)
                .FirstOrDefaultAsync(u => u.Id == id);
            if (unit == null)
                return NotFound("Unit not found");
            if (unit.Products.Any() || unit.UnitConversionBaseUnits.Any() || unit.UnitConversionFromUnits.Any())
            {
                return BadRequest("Cannot delete unit because it is is use");
            }
            _context.Units.Remove(unit);
            await _context.SaveChangesAsync();
            return Ok("Unit deleted successfully");
        }
    }
}
