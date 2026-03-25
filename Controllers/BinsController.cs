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
    public class BinsController : ControllerBase
    {
        private readonly WmsContext _context;
        private readonly IMapper _mapper;

        public BinsController(WmsContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

       
        [HttpGet]
        public async Task<IActionResult> GetAll([FromQuery] int? warehouseId)
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

            var user = await _context.Users
        .FirstOrDefaultAsync(x => x.Id == userId);

            if (user == null)
                return NotFound();
            var query = _context.Bins
                .Include(x => x.Warehouse)
                .Where(x => x.WarehouseId == user.WarehouseId)
                .AsQueryable();

            if (warehouseId.HasValue)
                query = query.Where(x => x.WarehouseId == warehouseId.Value);

            var result = await query
                .OrderBy(x => x.Code)
                .ToListAsync();

            return Ok(_mapper.Map<List<BinViewDto>>(result));
        }


        [HttpGet("available")]
        public async Task<IActionResult> GetAvailable([FromQuery] int warehouseId)
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

            var user = await _context.Users
        .FirstOrDefaultAsync(x => x.Id == userId);

            if (user == null)
                return NotFound();

            var bins = await _context.Bins
                .Where(x => x.WarehouseId == user.WarehouseId && x.Status == "Available")
                .OrderBy(x => x.Code)
                .ToListAsync();

            return Ok(_mapper.Map<List<BinViewDto>>(bins));
        }

      
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] BinCreateDto dto)
        {
            var exists = await _context.Bins
                .AnyAsync(x => x.Code == dto.Code && x.WarehouseId == dto.WarehouseId);
            if (exists)
                return BadRequest($"Bin '{dto.Code}' đã tồn tại trong kho này");

            var bin = _mapper.Map<Bin>(dto);
            bin.Status = "Available";
            _context.Bins.Add(bin);
            await _context.SaveChangesAsync();

            return Ok(_mapper.Map<BinViewDto>(bin));
        }

 
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] BinUpdateDto dto)
        {
            var bin = await _context.Bins.FindAsync(id);
            if (bin == null) return NotFound("Bin không tồn tại");

            bin.Code = dto.Code;
            bin.Name = dto.Name;
            bin.Status = dto.Status;
            await _context.SaveChangesAsync();

            return Ok(_mapper.Map<BinViewDto>(bin));
        }

        
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var bin = await _context.Bins.FindAsync(id);
            if (bin == null) return NotFound("Bin không tồn tại");

           
            var hasInventory = await _context.Inventories
                .AnyAsync(x => x.StoragePosition == bin.Code && x.WarehouseId == bin.WarehouseId);
            if (hasInventory)
                return BadRequest("Không thể xóa bin đang chứa hàng");

            _context.Bins.Remove(bin);
            await _context.SaveChangesAsync();
            return Ok(new { Message = "Xóa bin thành công" });
        }
    }
}