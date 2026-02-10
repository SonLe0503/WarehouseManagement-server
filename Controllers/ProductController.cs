using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using warehouseManagement.DTOs;
using warehouseManagement.Models;

namespace warehouseManagement.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ProductController : Controller
    {
        private readonly WmsContext _context;
        private readonly IMapper _mapper;

        public ProductController(WmsContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var products = await _context.Products
                .Include(p => p.Category)
                .Include(p => p.BaseUnit)
                .ToListAsync();

            var result = _mapper.Map<List<ProductDTO>>(products);
            return Ok(result);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var product = await _context.Products
                .Include(p => p.Category)
                .Include(p => p.BaseUnit)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (product == null)
            {
                return NotFound("Product not found");
            }
            return Ok(_mapper.Map<ProductDTO>(product));
        }
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateProductDTO dto)
        {
            if (await _context.Products.AnyAsync(p => p.Sku == dto.Sku))
            {
                return BadRequest("SKU already exists");
            }

            if(!await _context.Categories.AnyAsync(c => c.Id == dto.CategoryId))
            {
                return BadRequest("Category not found");
            }
            if(!await _context.Units.AnyAsync(u => u.Id == dto.BaseUnitId))
            {
                return BadRequest("Base unit not found");
            }
            var product = _mapper.Map<Product>(dto);
            _context.Products.Add(product);
            await _context.SaveChangesAsync();
            return CreatedAtAction(nameof(GetById), new { id = product.Id }, product.Id);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] UpdateProductDTO dto)
        {
            var product = await _context.Products.FindAsync(id);
            if (product == null)
            {
                return NotFound("Product not found");
            }

            if (dto.CategoryId.HasValue &&
                           !await _context.Categories.AnyAsync(c => c.Id == dto.CategoryId))
                return BadRequest("Category not found");
            if (dto.BaseUnitId.HasValue &&
                !await _context.Units.AnyAsync(u => u.Id == dto.BaseUnitId))
                return BadRequest("Unit not found");
            _mapper.Map(dto, product);
            await _context.SaveChangesAsync();
            return NoContent();
        }
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var product = await _context.Products.FindAsync(id);
            if (product == null)
                return NotFound("Product not found");
            product.Status = "INACTIVE";
            await _context.SaveChangesAsync();
            return NoContent();
        }
    }
}
