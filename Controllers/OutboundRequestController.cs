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
        [Authorize(Roles = "MANAGE,STAFF")]
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
        [HttpPost("{id}/ship")]
        [Authorize(Roles = "STAFF,MANAGE")]
        public async Task<IActionResult> ShipGoods(int id, [FromBody] PickedInboundRequestDTO dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var request = await _context.OutboundRequests
                .Include(r => r.OutboundItems)
                .FirstOrDefaultAsync(r => r.Id == id);

            if (request == null)
                return NotFound("Không tìm thấy đơn xuất kho");

            if (request.Status != "Approved")
                return BadRequest("Chỉ có thể xuất hàng cho đơn đã được duyệt");

            var itemIds = request.OutboundItems.Select(i => i.Id).ToHashSet();
            var invalidIds = dto.Items
                .Where(i => !itemIds.Contains(i.OutboundItemId))
                .Select(i => i.OutboundItemId)
                .ToList();

            if (invalidIds.Any())
                return BadRequest($"Các OutboundItemId không hợp lệ: {string.Join(", ", invalidIds)}");

            using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                foreach (var shipItem in dto.Items)
                {
                    if (shipItem.PickedQuantity <= 0)
                        return BadRequest("Số lượng xuất phải lớn hơn 0");

                    var item = request.OutboundItems
                        .First(i => i.Id == shipItem.OutboundItemId);

                    // Tìm tồn kho
                    var inventory = await _context.Inventories
                        .FirstOrDefaultAsync(inv =>
                            inv.ProductId == item.ProductId &&
                            inv.WarehouseId == request.WarehouseId &&
                            inv.StoragePosition == shipItem.StoragePosition);

                    if (inventory == null || inventory.Quantity < shipItem.PickedQuantity)
                        return BadRequest($"Không đủ tồn kho cho sản phẩm {item.ProductId}");

                    // Trừ tồn
                    inventory.Quantity -= shipItem.PickedQuantity;
                    inventory.UpdatedAt = DateTime.UtcNow;

                    // Update dòng xuất
                    item.PickedQuantity  = shipItem.PickedQuantity;
                    item.StoragePosition = shipItem.StoragePosition ?? item.StoragePosition;

                    if (shipItem.LineNote != null)
                        item.LineNote = shipItem.LineNote;

                    // Ghi lịch sử
                    var movement = new StockMovement
                    {
                        ProductId = item.ProductId,
                        WarehouseId = request.WarehouseId,
                        QuantityChange = -shipItem.PickedQuantity,
                        RefType = "OutboundRequest",
                        RefId = request.Id,
                        CreatedAt = DateTime.UtcNow
                    };

                    _context.StockMovements.Add(movement);
                }
                request.Status = "Completed";

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                return Ok(new
                {
                    Message = $"Xuất hàng cho đơn {request.RequestNo} thành công.",
                    NewStatus = request.Status
                });
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                return StatusCode(500, "Lỗi khi xử lý xuất hàng: " + ex.Message);
            }
        }
    }

}

