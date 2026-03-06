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
    public class InboundRequestController : Controller
    {
        private readonly WmsContext _context;
        private readonly IMapper _mapper;

        public InboundRequestController(WmsContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<InboundRequestDTO>>> GetInboundRequests()
        {
            var requests = await _context.InboundRequests
                .Include(r => r.InboundItems)
                .ThenInclude(i => i.Product)
                .Include(r => r.CreatedByNavigation)
                .Include(r => r.ApprovedByNavigation)
                .Include(r => r.Warehouse)
                .ToListAsync();

            var requestDtos = _mapper.Map<List<InboundRequestDTO>>(requests);
            return Ok(requestDtos);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<InboundRequestDTO>> GetInboundRequestDetails(int id)
        {
            var request = await _context.InboundRequests
                .Include(r => r.InboundItems)
                .ThenInclude(i => i.Product)
                .Include(r => r.CreatedByNavigation)
                .Include(r => r.ApprovedByNavigation)
                .Include(r => r.Warehouse)
                .FirstOrDefaultAsync(r => r.Id == id);

            if (request == null)
            {
                return NotFound();
            }

            var requestDto = _mapper.Map<InboundRequestDTO>(request);
            return Ok(requestDto);
        }
        [HttpPost("{id}/approval")]
        [Authorize(Roles = "MANAGE,STAFF")]
        public async Task<IActionResult> ApproveOrReject(int id, [FromBody] ApproveInboundRequestDTO dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var request = await _context.InboundRequests
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
                    request.ApprovedAt = DateTime.UtcNow;
                }
                else
                {
                    request.Status = "Rejected";

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

                return StatusCode(500, "Lỗi khi xử lý duyệt đơn: " + ex.Message);
            }
        }


        [HttpPost("{id}/receive")]
        [Authorize(Roles = "STAFF,MANAGE")]

        public async Task<IActionResult> ReceiveGoods(int id, [FromBody] ReceiveInboundRequestDto dto)
        {

            if (!ModelState.IsValid) return BadRequest(ModelState);

            var request = await _context.InboundRequests
                .Include(r => r.InboundItems)
                .FirstOrDefaultAsync(r => r.Id == id);

            if (request == null) return NotFound("Không tìm thấy đơn nhập kho");
                
            if (request.Status != "Approved") 
                return BadRequest("Chỉ có thể nhận hàng cho đơn đã được duyệt");


            var itemIds = request.InboundItems.Select(i => i.Id).ToHashSet();
            var invalidIds = dto.Items
            .Where(i => !itemIds.Contains(i.InboundItemId))
            .Select(i => i.InboundItemId)
            .ToList();


            if (invalidIds.Any())
            {
                return BadRequest($"Các InboundItemId không hợp lệ: {string.Join(", ", invalidIds)}");
            }
            using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
   
                    foreach(var receiveItem in dto.Items)
                    {
                        var item = request.InboundItems.First(i => i.Id == receiveItem.InboundItemId);

                        item.ReceivedQuantity = receiveItem.ReceivedQuantity;
                        item.StoragePosition = receiveItem.StoragePosition ?? item.StoragePosition;
                    if (receiveItem.LineNote != null)
                            item.LineNote = receiveItem.LineNote;

                    var inventory = await _context.Inventories
                        .FirstOrDefaultAsync(inv =>

                             inv.ProductId == item.ProductId &&
                             inv.WarehouseId == request.WarehouseId &&
                             inv.UnitId == item.UnitId &&
                             inv.StoragePosition == receiveItem.StoragePosition);


                    if (inventory != null)
                    {
                        inventory.Quantity += receiveItem.ReceivedQuantity;
                        inventory.UpdatedAt = DateTime.UtcNow;
                        inventory.StoragePosition = receiveItem.StoragePosition ?? inventory.StoragePosition;
                    }
                    else
                    {
                        var newInventory = new Inventory
                        {
                            ProductId = item.ProductId,
                            WarehouseId = request.WarehouseId,
                            UnitId = item.UnitId,
                            Quantity = receiveItem.ReceivedQuantity,
                            StoragePosition = receiveItem.StoragePosition,
                            UpdatedAt = DateTime.UtcNow

                        };
                        _context.Inventories.Add(newInventory);


                    }

                    var movement = new StockMovement
                    {
                        ProductId = item.ProductId,
                        WarehouseId = request.WarehouseId,
                        QuantityChange = receiveItem.ReceivedQuantity,
                        RefType = "InboundRequest",
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
                    Message = $"Nhận hàng cho đơn {request.RequestNo} thành công.",
                    NewStatus = request.Status
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, "Lỗi khi xử lý nhận hàng: " + ex.Message);

            }

        }
    }
}
 