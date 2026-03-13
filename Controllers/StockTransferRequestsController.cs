using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using warehouseManagement.DTOs.StockTransferRequests;
using warehouseManagement.Models;

namespace warehouseManagement.Controllers
{
    [Authorize(Roles = "STAFF")]
    [ApiController]
    [Route("api/stock-transfer-requests")]
    public class StockTransferRequestsController : Controller
    {
        private readonly WmsContext _context;
        private readonly IMapper _mapper;

        public StockTransferRequestsController(WmsContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        [HttpGet("my")]
        public async Task<ActionResult<IEnumerable<StockTransferViewDto>>> GetMyRequests()
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);

            var requests = await _context.StockTransferRequests
                .AsNoTracking()
                .Include(r => r.FromWarehouse)
                .Include(r => r.ToWarehouse)
                .Where(r => r.CreatedBy == userId)
                .OrderByDescending(r => r.CreatedAt)
                .ToListAsync();

            return Ok(_mapper.Map<List<StockTransferViewDto>>(requests));
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] StockTransferCreateDto dto)
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);

            if (dto.FromWarehouseId == dto.ToWarehouseId)
                return BadRequest("Kho nguồn và kho đích phải khác nhau");

            using var transaction = await _context.Database.BeginTransactionAsync();

            var request = _mapper.Map<StockTransferRequest>(dto);
            request.CreatedBy = userId;
            request.CreatedAt = DateTime.Now;
            request.Status = "Pending";
            request.TransferNo = $"TRF-{DateTime.Now.Ticks.ToString()[^6..]}";

            _context.StockTransferRequests.Add(request);
            await _context.SaveChangesAsync();

            if (dto.Items != null && dto.Items.Any())
            {
                foreach (var itemDto in dto.Items)
                {
                    var item = new StockTransferItem
                    {
                        StockTransferRequestId = request.Id,
                        ProductId = itemDto.ProductId,
                        Quantity = itemDto.Quantity,
                        LineNote = itemDto.LineNote
                    };

                    _context.StockTransferItems.Add(item);
                }

                await _context.SaveChangesAsync();
            }

            var approval = new Approval
            {
                RefType = "StockTransferRequest",
                RefId = request.Id,
                Status = "Pending",
                CurrentStep = 1
            };

            _context.Approvals.Add(approval);
            await _context.SaveChangesAsync();

            await transaction.CommitAsync();

            return Ok(new { request.Id, request.TransferNo });
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);

            var request = await _context.StockTransferRequests
                .Include(r => r.StockTransferItems)
                    .ThenInclude(i => i.Product)
                        .ThenInclude(p => p.BaseUnit)
                .Include(r => r.StockTransferItems)
                    .ThenInclude(i => i.Product)
                        .ThenInclude(p => p.Category)
                .Include(r => r.FromWarehouse)
                .Include(r => r.ToWarehouse)
                .FirstOrDefaultAsync(r => r.Id == id);

            if (request == null)
                return NotFound();

            if (request.CreatedBy != userId)
                return Forbid();

            var dto = _mapper.Map<StockTransferViewDto>(request);

            return Ok(dto);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] StockTransferCreateDto dto)
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);

            if (dto.FromWarehouseId == dto.ToWarehouseId)
                return BadRequest("Kho nguồn và kho đích phải khác nhau");

            var request = await _context.StockTransferRequests
                .Include(r => r.StockTransferItems)
                .FirstOrDefaultAsync(r => r.Id == id);

            if (request == null)
                return NotFound();

            if (request.CreatedBy != userId)
                return Forbid();

            if (request.Status != "Pending" && request.Status != "Rejected")
                return BadRequest("Chỉ có thể chỉnh sửa phiếu ở trạng thái Pending hoặc Rejected");

            _context.StockTransferItems.RemoveRange(request.StockTransferItems);

            request.FromWarehouseId = dto.FromWarehouseId;
            request.ToWarehouseId = dto.ToWarehouseId;
            request.Note = dto.Note;
            request.Status = "Pending";

            if (dto.Items != null && dto.Items.Any())
            {
                foreach (var itemDto in dto.Items)
                {
                    var item = new StockTransferItem
                    {
                        StockTransferRequestId = request.Id,
                        ProductId = itemDto.ProductId,
                        Quantity = itemDto.Quantity,
                        LineNote = itemDto.LineNote
                    };

                    _context.StockTransferItems.Add(item);
                }
            }

            await _context.SaveChangesAsync();

            return Ok();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);

            var request = await _context.StockTransferRequests
                .Include(r => r.StockTransferItems)
                .FirstOrDefaultAsync(r => r.Id == id);

            if (request == null)
                return NotFound();

            if (request.CreatedBy != userId)
                return Forbid();

            if (request.Status == "Approved" || request.Status == "InTransit" || request.Status == "Completed")
                return BadRequest("Không thể xóa phiếu đã duyệt hoặc đang xử lý");

            using var transaction = await _context.Database.BeginTransactionAsync();

            var approval = await _context.Approvals
                .Include(a => a.ApprovalLogs)
                .FirstOrDefaultAsync(a => a.RefType == "StockTransferRequest" && a.RefId == id);

            if (approval != null)
            {
                _context.ApprovalLogs.RemoveRange(approval.ApprovalLogs);
                _context.Approvals.Remove(approval);
            }

            _context.StockTransferItems.RemoveRange(request.StockTransferItems);
            _context.StockTransferRequests.Remove(request);

            await _context.SaveChangesAsync();
            await transaction.CommitAsync();

            return Ok();
        }
    }
}
