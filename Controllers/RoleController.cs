using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using warehouseManagement.Models;

namespace warehouseManagement.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class RoleController : Controller
    {
        private readonly WmsContext _context;
        public RoleController(WmsContext context) {
            _context = context;
        }

        [Authorize]

        [HttpGet]
        public async Task<IActionResult> GetAllRoles()
        {
            var roles = await _context.Roles
                .OrderBy(r => r.Id)
                .Select(r => new
                {
                    r.Id,
                    r.Name
                })
                .ToListAsync();

            return Ok(roles);
        }


        [HttpPost]
        public async Task<IActionResult> CreateRole([FromBody] DTOs.CreateRoleDTO dto)
        {
            if (string.IsNullOrWhiteSpace(dto.Name))
                return BadRequest("Role name is required");

            var roleName = dto.Name.Trim().ToUpper();

            var exists = await _context.Roles
                .AnyAsync(r => r.Name == roleName);

            if (exists)
                return BadRequest("Role already exists");

            var role = new Role
            {
                Name = roleName
            };

            _context.Roles.Add(role);
            await _context.SaveChangesAsync();

            return Ok(new
            {
                message = "Create role successfully",
                roleId = role.Id,
                roleName = role.Name
            });
        }
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteRole(int id)
        {
            var role = await _context.Roles
                .Include(r => r.Users)
                .FirstOrDefaultAsync(r => r.Id == id);

            if (role == null)
                return NotFound("Role not found");

            if (role.Users.Any())
            {
                return BadRequest(new
                {
                    message = "Cannot delete role because it is assigned to users",
                    userCount = role.Users.Count
                });
            }

            _context.Roles.Remove(role);
            await _context.SaveChangesAsync();

            return Ok("Delete role successfully");
        }
    }
}
