using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using warehouseManagement.Models;
using WarehouseManagement.DTOs.InboundRequests;

[Authorize(Roles = "PURCHASE")]
[ApiController]
[Route("api/inbound-requests")]
public class InboundRequestsController : ControllerBase
{
    private readonly WmsContext _context;
    private readonly IMapper _mapper;

    public InboundRequestsController(WmsContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    [HttpGet("my")]
    public async Task<ActionResult<IEnumerable<InboundRequestViewDto>>> GetMyRequests()
    {
        var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);

        var requests = await _context.InboundRequests
            .AsNoTracking()
            .Where(r => r.CreatedBy == userId)
            .OrderByDescending(r => r.CreatedAt)
            .ToListAsync();

        return Ok(_mapper.Map<List<InboundRequestViewDto>>(requests));
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] InboundRequestCreateDto dto)
    {
        var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);

        using var transaction = await _context.Database.BeginTransactionAsync();

        var request = _mapper.Map<InboundRequest>(dto);
        request.CreatedBy = userId;
        request.CreatedAt = DateTime.Now;
        request.Status = "Pending";
        request.RequestNo = $"IN-{DateTime.Now.Ticks.ToString()[^6..]}";

        _context.InboundRequests.Add(request);
        await _context.SaveChangesAsync();

       
        if (dto.Items != null && dto.Items.Any())
        {
            foreach (var itemDto in dto.Items)
            {
                var item = new InboundItem
                {
                    InboundRequestId = request.Id,
                    ProductId = itemDto.ProductId,
                    Quantity = itemDto.Quantity,
                    UnitId = itemDto.UnitId,
                    LineNote = itemDto.LineNote
                };
                _context.InboundItems.Add(item);
            }
            await _context.SaveChangesAsync();
        }

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

        return Ok(new { request.Id, request.RequestNo });
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] InboundRequestCreateDto dto)
    {
        var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);

        var request = await _context.InboundRequests
            .Include(r => r.InboundItems)
            .FirstOrDefaultAsync(r => r.Id == id);

        if (request == null)
            return NotFound();

        if (request.CreatedBy != userId)
            return Forbid();

        if (request.Status == "Approved" || request.Status == "Completed")
            return BadRequest();
     
        _context.InboundItems.RemoveRange(request.InboundItems);     
        request.SupplierName = dto.SupplierName;
        request.Note = dto.Note;
        request.WarehouseId = dto.WarehouseId;
        request.Status = "Pending";


        if (dto.Items != null && dto.Items.Any())
        {
            foreach (var itemDto in dto.Items)
            {
                var item = new InboundItem
                {
                    InboundRequestId = request.Id,
                    ProductId = itemDto.ProductId,
                    Quantity = itemDto.Quantity,
                    UnitId = itemDto.UnitId,
                    LineNote = itemDto.LineNote
                };
                _context.InboundItems.Add(item);
            }
        }

        await _context.SaveChangesAsync();

        return Ok();
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);

        var request = await _context.InboundRequests
            .Include(r => r.InboundItems)
                .ThenInclude(i => i.Unit)
            .Include(r => r.InboundItems)
                .ThenInclude(i => i.Product)
                  .ThenInclude(p => p.BaseUnit)
            .Include(r => r.InboundItems)
                .ThenInclude(i => i.Product)
                  .ThenInclude(p => p.Category)
            .FirstOrDefaultAsync(r => r.Id == id);

        if (request == null)
            return NotFound();

        if (request.CreatedBy != userId)
            return Forbid();

        var lastLog = await _context.ApprovalLogs
            .Where(l => l.Approval.RefType == "InboundRequest" && l.Approval.RefId == id)
            .OrderByDescending(l => l.ActionAt)
            .Select(l => new
            {
                l.Action,
                l.Comment,
                l.ActionAt
            })
            .FirstOrDefaultAsync();

        var dto = _mapper.Map<InboundRequestViewDto>(request);

        return Ok(new
        {
            Data = dto, 
            History = lastLog
        });
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);

        var request = await _context.InboundRequests
            .Include(r => r.InboundItems)
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

        return Ok();
    }
}
