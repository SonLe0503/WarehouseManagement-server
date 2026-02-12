using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using warehouseManagement.Models;
using warehouseManagement.Services;

namespace warehouseManagement.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UserController : Controller
    {
        private readonly WmsContext _context;
        private readonly IMapper _mapper;
        private readonly IEmailService _emailService;

        public UserController(WmsContext context, IMapper mapper, IEmailService emailService)
        {
            _context = context;
            _mapper = mapper;
            _emailService = emailService;
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
            if (string.IsNullOrWhiteSpace(dto.Username))
                return BadRequest("Username is required");

            var tempPassword = Guid.NewGuid().ToString("N").Substring(0, 8);

            var exists = await _context.Users
                .AnyAsync(u => u.Username == dto.Username);

            if (exists)
                return BadRequest("Username already exists");

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
                Status = string.IsNullOrEmpty(dto.Status) ? "ACIT" : dto.Status,
                CreatedAt = DateTime.UtcNow,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(tempPassword)
            };

            foreach (var role in roles)
            {
                user.Roles.Add(role);
            }

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            var body = $@"
                <h3>Tài khoản WMS System</h3>
                <p>Xin chào,</p>
                <p>Tài khoản của bạn đã được tạo thành công:</p>
                <ul>
                     <li><b>Username:</b> {dto.Username}</li>
                     <li><b>Password tạm:</b> {tempPassword}</li>
                </ul>
                <p>Vui lòng đăng nhập và đổi mật khẩu ngay.</p>
            ";

            await _emailService.SendAsync(
                dto.Email,
                "Thông tin tài khoản HR System",
                body
            );

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

            var oldEmail = user.Email;
            var oldStatus = user.Status;
            var oldRoleIds = user.Roles.Select(r => r.Id).ToList();

            _mapper.Map(dto, user);
            user.UpdatedAt = DateTime.UtcNow;

            bool isAdmin = user.Roles.Any(r => r.Id == 1);
            bool roleChanged = false;

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

                roleChanged = !oldRoleIds.OrderBy(x => x)
                             .SequenceEqual(roleIds.OrderBy(x => x));
            }

            await _context.SaveChangesAsync();

            // ===== SEND EMAIL IF SOMETHING IMPORTANT CHANGED =====
            bool emailChanged = oldEmail != user.Email;
            bool statusChanged = oldStatus != user.Status;

            if (emailChanged || roleChanged || statusChanged)
            {
                var roleNames = user.Roles.Select(r => r.Name);

                var body = $@"
            <h3>Thông tin tài khoản WMS System đã được cập nhật</h3>
            <p>Xin chào {user.Username},</p>
            <p>Tài khoản của bạn vừa được cập nhật:</p>
            <ul>
                <li><b>Email:</b> {user.Email}</li>
                <li><b>Status:</b> {user.Status}</li>
                <li><b>Roles:</b> {string.Join(", ", roleNames)}</li>
            </ul>
            <p>Nếu bạn không thực hiện thay đổi này, vui lòng liên hệ Admin.</p>
        ";

                await _emailService.SendAsync(
                    user.Email,
                    "Tài khoản WMS System đã được cập nhật",
                    body
                );
            }

            return Ok("Update user successfully");
        }


        [Authorize(Roles = "ADMIN")]
        [HttpPut("deactivate/{id}")]
        public async Task<IActionResult> DeactivateUser(int id)
        {
            var user = await _context.Users
                .Include(u => u.Roles)
                .FirstOrDefaultAsync(u => u.Id == id);

            if (user == null)
                return NotFound("User not found");

            if (user.Roles.Any(r => r.Id == 1))
                return BadRequest("Cannot deactivate admin account");

            if (user.Status == "Inactive")
                return BadRequest("User already inactive");

            user.Status = "Inactive";
            user.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return Ok("User deactivated successfully");
        }

        [Authorize(Roles = "ADMIN")]
        [HttpPut("activate/{id}")]
        public async Task<IActionResult> ActivateUser(int id)
        {
            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Id == id);

            if (user == null)
                return NotFound("User not found");

            if (user.Status == "Active")
                return BadRequest("User already active");

            user.Status = "Active";
            user.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return Ok("User activated successfully");
        }

    }
}
