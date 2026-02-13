using System.Security.Claims;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using warehouseManagement.DTOs;
using warehouseManagement.Models;

namespace warehouseManagement.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class OutboundRequestController : Controller
    {
        private readonly WmsContext _context;
        private readonly IMapper _mapper;
    

    public OutboundRequestController(WmsContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<OutboundRequestDTO>>> GetOutboundRequests()
        {
            var requests = await _context.OutboundRequests
                .Include(r => r.OutboundItems)
                .ThenInclude(i => i.Product)
                .Include(r => r.CreatedByNavigation)
                .Include(r => r.ApprovedByNavigation)
                .Include(r => r.Warehouse)
                .ToListAsync();

            var requestDtos = _mapper.Map<List<OutboundRequestDTO>>(requests);
            return Ok(requestDtos);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<OutboundRequestDTO>> GetOutboundRequestDetails(int id)
        {
            var request = await _context.OutboundRequests
                .Include(r => r.OutboundItems)
                .ThenInclude(i => i.Product)
                .Include(r => r.CreatedByNavigation)
                .Include(r => r.ApprovedByNavigation)
                .Include(r => r.Warehouse)
                .FirstOrDefaultAsync(r => r.Id == id);

            if (request == null)
            {
                return NotFound();
            }

            var requestDto = _mapper.Map<OutboundRequestDTO>(request);
            return Ok(requestDto);
        }
        [HttpPost("{id}/approval")]
        [Authorize(Roles = "MANAGER,STAFF")]
        public async Task<IActionResult> ApproveOrReject(int id, [FromBody] ApproveOutboundRequestDTO dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var request = await _context.OutboundRequests
                .FirstOrDefaultAsync(r => r.Id == id);

            if (request == null) return NotFound("Không tìm thấy đơn nhập kho");

            var currentUserId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");

            if (request.Status != "Pending")
                return BadRequest("Chỉ có thể duyệt/từ chối đơn ở trạng thái Pending");

            if (dto.Action != "Approve" && dto.Action != "Reject")
                return BadRequest("Action phải là 'Approve' hoặc 'Reject'");

            using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                if (dto.Action == "Approve")
                {
                    request.Status = "Approved";
                    request.ApprovedBy = currentUserId;
                    //request.ApprovedAt = DateTime.UtcNow;
                }
                else 
                {
                    request.Status = "Rejected";
                    //request.RejectedReason = dto.RejectReason;
                }

                var log = new ApprovalLog
                {
                    Action = dto.Action == "Approve" ? "Approved" : "Rejected",
                    ActionBy = currentUserId,
                    ActionAt = DateTime.UtcNow,
                    Comment = dto.Comment ?? dto.RejectReason
                };
                _context.ApprovalLogs.Add(log);

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                return Ok(new
                {
                    Message = $"Đơn {request.RequestNo} đã được {dto.Action.ToLower()} thành công.",
                    NewStatus = request.Status
                });
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                // Log lỗi nếu cần
                return StatusCode(500, "Lỗi khi xử lý duyệt đơn: " + ex.Message);
            }
        }
}

}

