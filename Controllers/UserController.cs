using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using warehouseManagement.Models;

namespace warehouseManagement.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UserController : Controller
    {
        private readonly WmsContext _context;
        private readonly IMapper _mapper;

        public UserController(WmsContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        [Authorize]
        [HttpGet]
        public IActionResult GetAllUsers()
        {
            var users = _context.Users.
                Include(u => u.Roles).ToList();
            var userDTOs = _mapper.Map<List<DTOs.UserDTO>>(users);
            return Ok(userDTOs);
        }

        [Authorize(Roles = "ADMIN")]
        [HttpPost]
        public async Task<IActionResult> CreateUser([FromBody] DTOs.CreateUserDTO dto)
        {
            // 1. Validate cơ bản
            if (string.IsNullOrWhiteSpace(dto.Username))
                return BadRequest("Username is required");

            if (string.IsNullOrWhiteSpace(dto.Password))
                return BadRequest("Password is required");

            // 2. Check username tồn tại
            var exists = await _context.Users
                .AnyAsync(u => u.Username == dto.Username);

            if (exists)
                return BadRequest("Username already exists");

            // 3. Validate role
            List<Role> roles = new();

            if (dto.RoleIds != null && dto.RoleIds.Any())
            {
                var roleIds = dto.RoleIds.Distinct().ToList();

                if (roleIds.Contains(1))
                {
                    var adminExists = await _context.Users
                        .AnyAsync(u => u.Roles.Any(r => r.Id == 1));

                    if (adminExists)
                    {
                        return BadRequest(new
                        {
                            message = "Admin role can only be assigned to one user"
                        });
                    }
                }

                roles = await _context.Roles
                    .Where(r => roleIds.Contains(r.Id))
                    .ToListAsync();

                if (roles.Count != roleIds.Count)
                {
                    var invalidRoleIds = roleIds.Except(roles.Select(r => r.Id));
                    return BadRequest(new
                    {
                        message = "Invalid role id detected",
                        invalidRoleIds
                    });
                }
            }
            var user = new User
            {
                Username = dto.Username,
                Email = dto.Email,
                Status = string.IsNullOrEmpty(dto.Status) ? "Active" : dto.Status,
                CreatedAt = DateTime.UtcNow,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password)
            };

            foreach (var role in roles)
            {
                user.Roles.Add(role);
            }

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            return Ok(new
            {
                message = "Create user successfully",
                userId = user.Id
            });
        }

        [Authorize]
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateUser(int id, [FromBody] DTOs.UpdateUserDTO dto)
        {
            var user = await _context.Users
                .Include(u => u.Roles)
                .FirstOrDefaultAsync(u => u.Id == id);

            if (user == null)
                return NotFound("User not found");

            _mapper.Map(dto, user);
            user.UpdatedAt = DateTime.UtcNow;
            bool isAdmin = user.Roles.Any(r => r.Id == 1);

            if (dto.RoleIds != null && !isAdmin)
            {
                var roleIds = dto.RoleIds.Distinct().ToList();

                var roles = await _context.Roles
                    .Where(r => roleIds.Contains(r.Id))
                    .ToListAsync();

                if (roles.Count != roleIds.Count)
                {
                    var foundRoleIds = roles.Select(r => r.Id);
                    var invalidRoleIds = roleIds.Except(foundRoleIds);

                    return BadRequest(new
                    {
                        message = "One or more roles are invalid",
                        invalidRoleIds
                    });
                }
                user.Roles.Clear();

                foreach (var role in roles)
                {
                    user.Roles.Add(role);
                }
            }

            await _context.SaveChangesAsync();

            return Ok("Update user successfully");
        }

        [Authorize(Roles = "ADMIN")]
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteUser(int id)
        {
            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Id == id);

            if (user == null)
                return NotFound("User not found");

            if (user.Roles.Any(r => r.Id == 1))
            {
                return BadRequest("Cannot delete admin account");
            }

            if (user.Status == "Deleted")
                return BadRequest("User already deleted");

            user.Status = "Deleted";
            user.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return Ok("Delete user successfully");
        }
    }
}
