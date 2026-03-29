using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Formats.Asn1;
using warehouseManagement.DTOs;
using warehouseManagement.Models;

namespace warehouseManagement.Controllers
{
    [ApiController]
    [Authorize]
    [Route("api/[controller]")]
    public class CategoryController : Controller
    {
        private readonly WmsContext _context;
        public CategoryController(WmsContext context)
        {
            _context = context;
        }

        [HttpGet]

        public async Task<IActionResult> GetAllCategories()
        {
            var categories = await _context.Categories.ToListAsync();

            var lookup = categories.ToDictionary(
                c => c.Id,
                c => new CategoryDTO
                {
                    Id = c.Id,
                    Name = c.Name,
                    ParentId = c.ParentId
                });

            var roots = new List<CategoryDTO>();

            foreach (var cat in lookup.Values)
            {
                if (cat.ParentId == null)
                    roots.Add(cat);
                else if (lookup.ContainsKey(cat.ParentId.Value))
                    lookup[cat.ParentId.Value].Children.Add(cat);
            }

            return Ok(roots);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var category = await _context.Categories
                .Include(c => c.Parent)
                .Include(c => c.InverseParent)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (category == null)
                return NotFound(new { message = "Category not found" });

            var result = new CategoryDetailDTO
            {
                Id = category.Id,
                Name = category.Name,
                ParentId = category.ParentId,
                ParentName = category.Parent?.Name,
                Children = category.InverseParent.Select(c => new CategoryDTO
                {
                    Id = c.Id,
                    Name = c.Name,
                    ParentId = c.ParentId
                }).ToList()
            };

            return Ok(result);
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateCategoryDTO dto)
        {
            if (string.IsNullOrWhiteSpace(dto.Name))
                return BadRequest(new { message = "Name is required" });

            if (await _context.Categories.AnyAsync(c => c.Name == dto.Name))
                return BadRequest(new { message = "Category name already exists" });

            if (dto.ParentId.HasValue && !await _context.Categories.AnyAsync(c => c.Id == dto.ParentId))
            {
                return BadRequest(new { message = "Parent category not found" });
            }

            var category = new Category
            {
                Name = dto.Name,
                ParentId = dto.ParentId,
                CreatedAt = DateTime.UtcNow
            };
            _context.Categories.Add(category);
            await _context.SaveChangesAsync();
            return Ok(category);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] UpdateCategoryDTO dto)
        {
            var category = await _context.Categories.FindAsync(id);
            if (category == null)
            {
                return NotFound(new { message = "Category not found" });
            }
            if (dto.ParentId == id)
            {
                return BadRequest(new { message = "Category cannot be its own parent" });
            }
            if (dto.ParentId.HasValue && !await _context.Categories.AnyAsync(c => c.Id == dto.ParentId))
            {
                return BadRequest(new { message = "Parent category not found" });
            }

            category.Name = dto.Name;
            category.ParentId = dto.ParentId;
            await _context.SaveChangesAsync();
            return Ok(category);
        }
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var category = await _context.Categories
                .Include(c => c.InverseParent)
                .Include(c => c.Products)
                .FirstOrDefaultAsync(c => c.Id == id);
            if (category == null)
            {
                return NotFound(new { message = "Category not found" });
            }
            if (category.InverseParent.Any())
            {
                return BadRequest(new { message = "Cannot delete category with subcategories" });
            }
            if (category.Products.Any())
            {
                return BadRequest(new { message = "Cannot delete category that has products" });
            }
            _context.Categories.Remove(category);
            await _context.SaveChangesAsync();
            return Ok(new { message = "Category deleted successfully" });
        }
    }
}
