using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using warehouseManagement.DTOs.OutboundRequests;
using warehouseManagement.Models;

namespace warehouseManagement.Controllers
{
    [Authorize(Roles = "SALE")]
    [ApiController]
    [Route("api/outbound-requests")]
    public class OutboundRequestsController : Controller
    {
        private readonly WmsContext _context;
        private readonly IMapper _mapper;

        public OutboundRequestsController(WmsContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        [HttpGet("my")]
        public async Task<ActionResult<IEnumerable<OutboundRequestViewDto>>> GetMyRequests()
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);

            var requests = await _context.OutboundRequests
                .AsNoTracking()
                .Where(r => r.CreatedBy == userId)
                .OrderByDescending(r => r.CreatedAt)
                .ToListAsync();

            return Ok(_mapper.Map<List<OutboundRequestViewDto>>(requests));
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] OutboundRequestCreateDto dto)
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);

            using var transaction = await _context.Database.BeginTransactionAsync();

            var request = _mapper.Map<OutboundRequest>(dto);
            request.CreatedBy = userId;
            request.CreatedAt = DateTime.Now;
            request.Status = "Pending";
            request.RequestNo = $"OUT-{DateTime.Now.Ticks.ToString()[^6..]}";

            _context.OutboundRequests.Add(request);
            await _context.SaveChangesAsync();

            if (dto.Items != null && dto.Items.Any())
            {
                foreach (var itemDto in dto.Items)
                {
                    var item = new OutboundItem
                    {
                        OutboundRequestId = request.Id,
                        ProductId = itemDto.ProductId,
                        Quantity = itemDto.Quantity,
                        LineNote = itemDto.LineNote
                    };

                    _context.OutboundItems.Add(item);
                }

                await _context.SaveChangesAsync();
            }

            var approval = new Approval
            {
                RefType = "OutboundRequest",
                RefId = request.Id,
                Status = "Pending",
                CurrentStep = 1
            };

            _context.Approvals.Add(approval);
            await _context.SaveChangesAsync();

            await transaction.CommitAsync();

            return Ok(new { request.Id, request.RequestNo });
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);

            var request = await _context.OutboundRequests
                .Include(r => r.OutboundItems)
                    .ThenInclude(i => i.Product)
                        .ThenInclude(p => p.BaseUnit)
                .Include(r => r.OutboundItems)
                    .ThenInclude(i => i.Product)
                        .ThenInclude(p => p.Category)
                .FirstOrDefaultAsync(r => r.Id == id);

            if (request == null)
                return NotFound();

            if (request.CreatedBy != userId)
                return Forbid();

            var dto = _mapper.Map<OutboundRequestViewDto>(request);

            return Ok(dto);
        }
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] OutboundRequestCreateDto dto)
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);

            var request = await _context.OutboundRequests
                .Include(r => r.OutboundItems)
                .FirstOrDefaultAsync(r => r.Id == id);

            if (request == null)
                return NotFound();

            if (request.CreatedBy != userId)
                return Forbid();

            if (request.Status == "Approved" || request.Status == "Completed")
                return BadRequest();

            _context.OutboundItems.RemoveRange(request.OutboundItems);

            request.CustomerName = dto.CustomerName;
            request.Note = dto.Note;
            request.WarehouseId = dto.WarehouseId;
            request.Status = "Pending";

            if (dto.Items != null && dto.Items.Any())
            {
                foreach (var itemDto in dto.Items)
                {
                    var item = new OutboundItem
                    {
                        OutboundRequestId = request.Id,
                        ProductId = itemDto.ProductId,
                        Quantity = itemDto.Quantity,
                        LineNote = itemDto.LineNote
                    };

                    _context.OutboundItems.Add(item);
                }
            }

            await _context.SaveChangesAsync();

            return Ok();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);

            var request = await _context.OutboundRequests
                .Include(r => r.OutboundItems)
                .FirstOrDefaultAsync(r => r.Id == id);

            if (request == null)
                return NotFound();

            if (request.CreatedBy != userId)
                return Forbid();

            if (request.Status == "Approved" || request.Status == "Completed")
                return BadRequest();

            using var transaction = await _context.Database.BeginTransactionAsync();

            var approval = await _context.Approvals
                .Include(a => a.ApprovalLogs)
                .FirstOrDefaultAsync(a => a.RefType == "OutboundRequest" && a.RefId == id);

            if (approval != null)
            {
                _context.ApprovalLogs.RemoveRange(approval.ApprovalLogs);
                _context.Approvals.Remove(approval);
            }

            _context.OutboundItems.RemoveRange(request.OutboundItems);
            _context.OutboundRequests.Remove(request);

            await _context.SaveChangesAsync();
            await transaction.CommitAsync();

            return Ok();
        }
    }
}
