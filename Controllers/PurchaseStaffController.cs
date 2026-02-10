using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using warehouseManagement.Models;
using static warehouseManagement.DTOs.PurchaseStaffDTO;

[Authorize]
[Route("api/[controller]")]
[ApiController]
public class PurchaseStaffController : ControllerBase
{
    private readonly WmsContext _context;
    private readonly IMapper _mapper;

    public PurchaseStaffController(WmsContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    // 1. Lấy danh sách phiếu nhập của tôi
    [HttpGet("my-requests")]
    public async Task<ActionResult<IEnumerable<InboundRequestViewDto>>> GetMyRequests()
    {
        var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);

        var requests = await _context.InboundRequests
            .Include(r => r.InboundItems)
            .Where(r => r.CreatedBy == userId)
            .OrderByDescending(r => r.CreatedAt)
            .ToListAsync();

        return Ok(_mapper.Map<List<InboundRequestViewDto>>(requests));
    }


    // 2. Tạo phiếu mua hàng mới (Trạng thái mặc định: Draft hoặc Pending)
    [HttpPost("create")]
    public async Task<IActionResult> CreateInboundRequest([FromBody] InboundRequestCreateDto dto)
    {
        var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);

        using var transaction = await _context.Database.BeginTransactionAsync();
        try
        {
            var request = _mapper.Map<InboundRequest>(dto);
            request.CreatedBy = userId;
            request.CreatedAt = DateTime.Now;
            request.Status = "Pending";
            request.RequestNo = "IN-" + DateTime.Now.Ticks.ToString().Substring(10);

            _context.InboundRequests.Add(request);
            await _context.SaveChangesAsync();

            var approval = new Approval
            {
                RefType = "InboundRequest",
                RefId = request.Id,
                Status = "Pending",
                CurrentStep = 1
            };

            _context.Approvals.Add(approval);
            await _context.SaveChangesAsync();
            await transaction.CommitAsync();

            return Ok(new { Message = "Tạo phiếu thành công", RequestNo = request.RequestNo });
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }


  
    [HttpPut("update/{id}")]
    public async Task<IActionResult> UpdateRequest(int id, [FromBody] InboundRequestCreateDto dto)
    {
       
        var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);

        var request = await _context.InboundRequests
            .Include(r => r.InboundItems)
            .FirstOrDefaultAsync(x => x.Id == id);

        if (request == null) return NotFound("Không tìm thấy phiếu nhập");

       
        if (request.CreatedBy != userId)
            return Forbid("Bạn không có quyền sửa phiếu này.");

        
        if (request.Status == "Approved" || request.Status == "Completed")
            return BadRequest("Không thể sửa phiếu đã được duyệt hoặc hoàn thành.");

        _mapper.Map(dto, request);
        request.Status = "Pending";

        _context.InboundItems.RemoveRange(request.InboundItems);
        request.InboundItems = _mapper.Map<List<InboundItem>>(dto.Items);

        await _context.SaveChangesAsync();
        return Ok(new { Message = "Cập nhật thành công" });
    }


  
    [HttpGet("{id}/details")]
    public async Task<IActionResult> GetDetails(int id)
    {
      
        var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);

        var request = await _context.InboundRequests
            .Include(r => r.InboundItems)
                .ThenInclude(i => i.Product)
            .FirstOrDefaultAsync(x => x.Id == id);

        if (request == null) return NotFound("Không tìm thấy phiếu nhập");

        
        if (request.CreatedBy != userId)
            return Forbid("Bạn không có quyền xem phiếu này.");

       
        var lastLog = await _context.ApprovalLogs
            .Where(l => l.Approval.RefId == id && l.Approval.RefType == "InboundRequest")
            .OrderByDescending(l => l.ActionAt)
            .Select(l => new { l.Action, l.Comment, l.ActionAt })
            .FirstOrDefaultAsync();

        return Ok(new
        {
            Data = request,
            History = lastLog
        });
    }

    // 5. Xóa phiếu (Chỉ được xóa khi status là 'Pending' hoặc 'Rejected')

    [HttpDelete("{id}/delete")]
    public async Task<IActionResult> DeleteRequest(int id)
    {
        var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);

        var request = await _context.InboundRequests
            .Include(r => r.InboundItems)
            .FirstOrDefaultAsync(x => x.Id == id);

        if (request == null) return NotFound("Không tìm thấy phiếu nhập");

        if (request.CreatedBy != userId)
            return Forbid("Bạn không có quyền xóa phiếu này.");

       
        if (request.Status == "Approved" || request.Status == "Completed")
            return BadRequest("Không thể xóa phiếu đã được duyệt hoặc hoàn thành.");

        using var transaction = await _context.Database.BeginTransactionAsync();
        try
        {
            
            var approval = await _context.Approvals
                .Include(a => a.ApprovalLogs)
                .FirstOrDefaultAsync(a => a.RefType == "InboundRequest" && a.RefId == id);

            if (approval != null)
            {
                _context.ApprovalLogs.RemoveRange(approval.ApprovalLogs);
                _context.Approvals.Remove(approval);
            }

            
            _context.InboundItems.RemoveRange(request.InboundItems);

            _context.InboundRequests.Remove(request);

            await _context.SaveChangesAsync();
            await transaction.CommitAsync();

            return Ok(new { Message = "Xóa phiếu thành công" });
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }
}