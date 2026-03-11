using System.Security.Claims;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using warehouseManagement.DTOs;
using warehouseManagement.DTOs.StockTransferRequests;
using warehouseManagement.Models;

namespace warehouseManagement.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class StockTransferController : Controller
    {
        private readonly WmsContext _context;
        private readonly IMapper _mapper;

        public StockTransferController(WmsContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<StockTransferViewDto>>> GetStockTransfers()
        {
            var requests = await _context.StockTransferRequests
                .Include(r => r.StockTransferItems)
                    .ThenInclude(i => i.Product)
                .Include(r => r.CreatedByNavigation)
                .Include(r => r.ApprovedByNavigation)
                .Include(r => r.FromWarehouse)
                .Include(r => r.ToWarehouse)
                .OrderByDescending(r => r.CreatedAt)
                .ToListAsync();

            var dtos = _mapper.Map<List<StockTransferViewDto>>(requests);
            return Ok(dtos);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<StockTransferViewDto>> GetStockTransferDetails(int id)
        {
            var request = await _context.StockTransferRequests
                .Include(r => r.StockTransferItems)
                    .ThenInclude(i => i.Product)
                        .ThenInclude(p => p.BaseUnit)
                .Include(r => r.StockTransferItems)
                    .ThenInclude(i => i.Product)
                        .ThenInclude(p => p.Category)
                .Include(r => r.CreatedByNavigation)
                .Include(r => r.ApprovedByNavigation)
                .Include(r => r.FromWarehouse)
                .Include(r => r.ToWarehouse)
                .FirstOrDefaultAsync(r => r.Id == id);

            if (request == null)
                return NotFound();

            var dto = _mapper.Map<StockTransferViewDto>(request);
            return Ok(dto);
        }

        [HttpPost("{id}/approval")]
        [Authorize(Roles = "MANAGE,STAFF")]
        public async Task<IActionResult> ApproveOrReject(int id, [FromBody] StockTransferApproveDto dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var request = await _context.StockTransferRequests
                .FirstOrDefaultAsync(r => r.Id == id);

            if (request == null) return NotFound("Không tìm thấy yêu cầu chuyển kho");

            var currentUserId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");

            if (request.Status != "Pending")
                return BadRequest("Chỉ có thể duyệt/từ chối yêu cầu ở trạng thái Pending");

            if (dto.Action != "Approve" && dto.Action != "Reject")
                return BadRequest("Action phải là 'Approve' hoặc 'Reject'");

            using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                if (dto.Action == "Approve")
                {
                    request.Status = "Approved";
                    request.ApprovedBy = currentUserId;
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
                    Message = $"Yêu cầu {request.TransferNo} đã được {dto.Action.ToLower()} thành công.",
                    NewStatus = request.Status
                });
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                return StatusCode(500, "Lỗi khi xử lý duyệt yêu cầu: " + ex.Message);
            }
        }

        [HttpPost("{id}/ship")]
        [Authorize(Roles = "STAFF,MANAGE")]
        public async Task<IActionResult> ShipGoods(int id, [FromBody] StockTransferShipDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var request = await _context.StockTransferRequests
                .Include(r => r.StockTransferItems)
                .FirstOrDefaultAsync(r => r.Id == id);

            if (request == null)
                return NotFound("Không tìm thấy yêu cầu chuyển kho");

            if (request.Status != "Approved")
                return BadRequest("Chỉ có thể xuất hàng cho yêu cầu đã được duyệt");

            var itemIds = request.StockTransferItems.Select(i => i.Id).ToHashSet();
            var invalidIds = dto.Items
                .Where(i => !itemIds.Contains(i.StockTransferItemId))
                .Select(i => i.StockTransferItemId)
                .ToList();

            if (invalidIds.Any())
                return BadRequest($"Các StockTransferItemId không hợp lệ: {string.Join(", ", invalidIds)}");

            using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                foreach (var shipItem in dto.Items)
                {
                    if (shipItem.PickedQuantity <= 0)
                        return BadRequest("Số lượng xuất phải lớn hơn 0");

                    var item = request.StockTransferItems
                        .First(i => i.Id == shipItem.StockTransferItemId);

                    var product = await _context.Products
                        .FirstAsync(p => p.Id == item.ProductId);

                    decimal baseQuantity = shipItem.PickedQuantity;

                    // Tìm tồn kho theo bin tại kho nguồn
                    var inventory = await _context.Inventories
                        .FirstOrDefaultAsync(inv =>
                            inv.ProductId == item.ProductId &&
                            inv.WarehouseId == request.FromWarehouseId &&
                            inv.StoragePosition == shipItem.StoragePosition);

                    if (inventory == null || inventory.Quantity < baseQuantity)
                        return BadRequest($"Không đủ tồn kho cho sản phẩm {item.ProductId} tại vị trí {shipItem.StoragePosition}");

                    // Trừ tồn kho nguồn
                    inventory.Quantity -= baseQuantity;
                    inventory.UpdatedAt = DateTime.UtcNow;

                    // Cập nhật item
                    item.FromStoragePosition = shipItem.StoragePosition;

                    if (shipItem.LineNote != null)
                        item.LineNote = shipItem.LineNote;

                    // Ghi nhận stock movement (xuất)
                    _context.StockMovements.Add(new StockMovement
                    {
                        ProductId = item.ProductId,
                        WarehouseId = request.FromWarehouseId,
                        QuantityChange = -baseQuantity,
                        StoragePosition = shipItem.StoragePosition,
                        RefType = "StockTransfer_Ship",
                        RefId = request.Id,
                        CreatedAt = DateTime.UtcNow
                    });
                }

                request.Status = "InTransit";

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                return Ok(new
                {
                    Message = $"Xuất hàng cho yêu cầu {request.TransferNo} thành công. Hàng đang vận chuyển.",
                    NewStatus = request.Status
                });
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                return StatusCode(500, "Lỗi khi xử lý xuất hàng: " + ex.Message);
            }
        }

        [HttpPost("{id}/receive")]
        [Authorize(Roles = "STAFF,MANAGE")]
        public async Task<IActionResult> ReceiveGoods(int id, [FromBody] StockTransferReceiveDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var request = await _context.StockTransferRequests
                .Include(r => r.StockTransferItems)
                .FirstOrDefaultAsync(r => r.Id == id);

            if (request == null)
                return NotFound("Không tìm thấy yêu cầu chuyển kho");

            if (request.Status != "InTransit")
                return BadRequest("Chỉ có thể nhận hàng cho yêu cầu đang vận chuyển (InTransit)");

            var itemIds = request.StockTransferItems.Select(i => i.Id).ToHashSet();
            var invalidIds = dto.Items
                .Where(i => !itemIds.Contains(i.StockTransferItemId))
                .Select(i => i.StockTransferItemId)
                .ToList();

            if (invalidIds.Any())
                return BadRequest($"Các StockTransferItemId không hợp lệ: {string.Join(", ", invalidIds)}");

            using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                foreach (var receiveItem in dto.Items)
                {
                    var item = request.StockTransferItems
                        .First(i => i.Id == receiveItem.StockTransferItemId);

                    item.ReceivedQuantity = receiveItem.BinQuantities.Sum(b => b.Quantity);
                    item.ToStoragePosition = receiveItem.BinQuantities.FirstOrDefault()?.StoragePosition;

                    if (receiveItem.LineNote != null)
                        item.LineNote = receiveItem.LineNote;

                    foreach (var binQty in receiveItem.BinQuantities)
                    {
                        decimal baseQuantity = binQty.Quantity;

                        // Cộng tồn kho đích
                        var inventory = await _context.Inventories
                            .FirstOrDefaultAsync(inv =>
                                inv.ProductId == item.ProductId &&
                                inv.WarehouseId == request.ToWarehouseId &&
                                inv.StoragePosition == binQty.StoragePosition);

                        if (inventory != null)
                        {
                            inventory.Quantity += baseQuantity;
                            inventory.UpdatedAt = DateTime.UtcNow;
                        }
                        else
                        {
                            _context.Inventories.Add(new Inventory
                            {
                                ProductId = item.ProductId,
                                WarehouseId = request.ToWarehouseId,
                                Quantity = baseQuantity,
                                StoragePosition = binQty.StoragePosition,
                                UpdatedAt = DateTime.UtcNow
                            });
                        }

                        // Ghi nhận stock movement (nhập)
                        _context.StockMovements.Add(new StockMovement
                        {
                            ProductId = item.ProductId,
                            WarehouseId = request.ToWarehouseId,
                            QuantityChange = baseQuantity,
                            StoragePosition = binQty.StoragePosition,
                            RefType = "StockTransfer_Receive",
                            RefId = request.Id,
                            CreatedAt = DateTime.UtcNow
                        });
                    }
                }

                request.Status = "Completed";

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                return Ok(new
                {
                    Message = $"Nhận hàng cho yêu cầu {request.TransferNo} thành công.",
                    NewStatus = request.Status
                });
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                return StatusCode(500, "Lỗi khi xử lý nhận hàng: " + ex.Message);
            }
        }
    }
}
