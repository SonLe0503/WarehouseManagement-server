using System.Security.Claims;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using warehouseManagement.DTOs.StockTransferRequests;
using warehouseManagement.Models;

namespace warehouseManagement.Controllers
{
    /// <summary>
    /// Chuyển hàng giữa 2 kho khác nhau — flow: Pending → Approve → Ship → Receive → Completed
    /// Route: /api/CrossWarehouseTransfer
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class CrossWarehouseTransferController : ControllerBase
    {
        private readonly WmsContext _context;
        private readonly IMapper _mapper;

        public CrossWarehouseTransferController(WmsContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<StockTransferViewDto>>> GetAll()
        {
            var requests = await _context.StockTransferRequests
                .Include(r => r.StockTransferItems).ThenInclude(i => i.Product)
                .Include(r => r.StockTransferItems).ThenInclude(i => i.Unit)
                .Include(r => r.CreatedByNavigation)
                .Include(r => r.ApprovedByNavigation)
                .Include(r => r.FromWarehouse)
                .Include(r => r.ToWarehouse)
                .Where(r => r.FromWarehouseId != r.ToWarehouseId) // chỉ lấy cross-warehouse
                .OrderByDescending(r => r.CreatedAt)
                .ToListAsync();

            return Ok(_mapper.Map<List<StockTransferViewDto>>(requests));
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<StockTransferViewDto>> GetById(int id)
        {
            var request = await _context.StockTransferRequests
                .Include(r => r.StockTransferItems).ThenInclude(i => i.Product).ThenInclude(p => p.BaseUnit)
                .Include(r => r.StockTransferItems).ThenInclude(i => i.Product).ThenInclude(p => p.Category)
                .Include(r => r.StockTransferItems).ThenInclude(i => i.Unit)
                .Include(r => r.CreatedByNavigation)
                .Include(r => r.ApprovedByNavigation)
                .Include(r => r.FromWarehouse)
                .Include(r => r.ToWarehouse)
                .FirstOrDefaultAsync(r => r.Id == id);

            if (request == null) return NotFound();
            return Ok(_mapper.Map<StockTransferViewDto>(request));
        }

        [HttpPost]
        [Authorize(Roles = "MANAGE,STAFF")]
        public async Task<IActionResult> Create([FromBody] StockTransferCreateDto dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            if (dto.FromWarehouseId == dto.ToWarehouseId)
                return BadRequest("Kho nguồn và kho đích không được trùng nhau. Dùng /api/StockTransfer cho chuyển bin nội bộ.");

            var userIdStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value
                         ?? User.FindFirst("sub")?.Value;
            if (!int.TryParse(userIdStr, out int currentUserId) || currentUserId == 0)
                return Unauthorized("Không xác định được người dùng.");

            var today = DateTime.UtcNow.ToString("yyyyMMdd");
            var prefix = $"CWT-{today}-";
            var lastNo = await _context.StockTransferRequests
                .Where(t => t.TransferNo.StartsWith(prefix))
                .OrderByDescending(t => t.TransferNo)
                .Select(t => t.TransferNo)
                .FirstOrDefaultAsync();
            int seq = 1;
            if (lastNo != null)
            {
                var parts = lastNo.Split('-');
                if (parts.Length == 3 && int.TryParse(parts[2], out int last)) seq = last + 1;
            }
            var transferNo = $"{prefix}{seq:D3}";

            var transfer = new StockTransferRequest
            {
                TransferNo = transferNo,
                FromWarehouseId = dto.FromWarehouseId,
                ToWarehouseId = dto.ToWarehouseId,
                Status = "Pending",
                Note = dto.Note,
                CreatedBy = currentUserId,
                CreatedAt = DateTime.UtcNow,
            };
            _context.StockTransferRequests.Add(transfer);
            await _context.SaveChangesAsync();

            foreach (var item in dto.Items)
            {
                var product = await _context.Products.FindAsync(item.ProductId);
                if (product == null)
                    return BadRequest($"Không tìm thấy sản phẩm Id={item.ProductId}");

                // Nếu FE không truyền UnitId thì dùng BaseUnitId
                int resolvedUnitId = item.UnitId > 0 ? item.UnitId : product.BaseUnitId;

                _context.StockTransferItems.Add(new StockTransferItem
                {
                    StockTransferRequestId = transfer.Id,
                    ProductId = item.ProductId,
                    UnitId = resolvedUnitId,
                    Quantity = item.Quantity,
                    LineNote = item.LineNote,
                });
            }
            await _context.SaveChangesAsync();

            return Ok(new { Message = "Tạo yêu cầu chuyển kho thành công", TransferNo = transferNo, TransferId = transfer.Id });
        }

        [HttpPost("{id}/approval")]
        [Authorize(Roles = "MANAGE,STAFF")]
        public async Task<IActionResult> ApproveOrReject(int id, [FromBody] StockTransferApproveDto dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var request = await _context.StockTransferRequests.FirstOrDefaultAsync(r => r.Id == id);
            if (request == null) return NotFound("Không tìm thấy yêu cầu chuyển kho");

            var userIdStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            int.TryParse(userIdStr, out int currentUserId);

            if (request.Status != "Pending")
                return BadRequest("Chỉ có thể duyệt/từ chối yêu cầu ở trạng thái Pending");
            if (dto.Action != "Approve" && dto.Action != "Reject")
                return BadRequest("Action phải là 'Approve' hoặc 'Reject'");

            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                request.Status = dto.Action == "Approve" ? "Approved" : "Rejected";
                if (dto.Action == "Approve") request.ApprovedBy = currentUserId;

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

                return Ok(new { Message = $"Yêu cầu {request.TransferNo} đã {request.Status}.", NewStatus = request.Status });
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                return StatusCode(500, "Lỗi: " + ex.Message);
            }
        }

        [HttpPost("{id}/ship")]
        [Authorize(Roles = "STAFF,MANAGE")]
        public async Task<IActionResult> ShipGoods(int id, [FromBody] StockTransferShipDto dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var request = await _context.StockTransferRequests
                .Include(r => r.StockTransferItems)
                .FirstOrDefaultAsync(r => r.Id == id);

            if (request == null) return NotFound("Không tìm thấy yêu cầu chuyển kho");
            if (request.Status != "Approved") return BadRequest("Chỉ có thể xuất hàng cho yêu cầu đã được duyệt");

            var itemIds = request.StockTransferItems.Select(i => i.Id).ToHashSet();
            var invalidIds = dto.Items.Where(i => !itemIds.Contains(i.StockTransferItemId)).Select(i => i.StockTransferItemId).ToList();
            if (invalidIds.Any()) return BadRequest($"StockTransferItemId không hợp lệ: {string.Join(", ", invalidIds)}");

            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                foreach (var shipItem in dto.Items)
                {
                    if (shipItem.PickedQuantity <= 0) return BadRequest("Số lượng xuất phải lớn hơn 0");

                    var item = request.StockTransferItems.First(i => i.Id == shipItem.StockTransferItemId);

                    var inventory = await _context.Inventories
                        .FirstOrDefaultAsync(inv =>
                            inv.ProductId == item.ProductId &&
                            inv.WarehouseId == request.FromWarehouseId &&
                            inv.StoragePosition == shipItem.StoragePosition);

                    if (inventory == null || inventory.Quantity < shipItem.PickedQuantity)
                        return BadRequest($"Không đủ tồn kho cho sản phẩm {item.ProductId} tại bin {shipItem.StoragePosition}");

                    inventory.Quantity -= shipItem.PickedQuantity;
                    inventory.UpdatedAt = DateTime.UtcNow;
                    item.FromStoragePosition = shipItem.StoragePosition;
                    if (shipItem.LineNote != null) item.LineNote = shipItem.LineNote;

                    _context.StockMovements.Add(new StockMovement
                    {
                        ProductId = item.ProductId,
                        WarehouseId = request.FromWarehouseId,
                        QuantityChange = -shipItem.PickedQuantity,
                        StoragePosition = shipItem.StoragePosition,
                        RefType = "StockTransfer_Ship",
                        RefId = request.Id,
                        CreatedAt = DateTime.UtcNow
                    });
                }

                request.Status = "InTransit";
                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                return Ok(new { Message = $"Xuất hàng {request.TransferNo} thành công. Hàng đang vận chuyển.", NewStatus = request.Status });
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                return StatusCode(500, "Lỗi: " + ex.Message);
            }
        }

        [HttpPost("{id}/receive")]
        [Authorize(Roles = "STAFF,MANAGE")]
        public async Task<IActionResult> ReceiveGoods(int id, [FromBody] StockTransferReceiveDto dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var request = await _context.StockTransferRequests
                .Include(r => r.StockTransferItems)
                .FirstOrDefaultAsync(r => r.Id == id);

            if (request == null) return NotFound("Không tìm thấy yêu cầu chuyển kho");
            if (request.Status != "InTransit") return BadRequest("Chỉ có thể nhận hàng cho yêu cầu đang vận chuyển (InTransit)");

            var itemIds = request.StockTransferItems.Select(i => i.Id).ToHashSet();
            var invalidIds = dto.Items.Where(i => !itemIds.Contains(i.StockTransferItemId)).Select(i => i.StockTransferItemId).ToList();
            if (invalidIds.Any()) return BadRequest($"StockTransferItemId không hợp lệ: {string.Join(", ", invalidIds)}");

            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                foreach (var receiveItem in dto.Items)
                {
                    var item = request.StockTransferItems.First(i => i.Id == receiveItem.StockTransferItemId);
                    item.ReceivedQuantity = receiveItem.BinQuantities.Sum(b => b.Quantity);
                    item.ToStoragePosition = receiveItem.BinQuantities.FirstOrDefault()?.StoragePosition;
                    if (receiveItem.LineNote != null) item.LineNote = receiveItem.LineNote;

                    foreach (var binQty in receiveItem.BinQuantities)
                    {
                        var inventory = await _context.Inventories
                            .FirstOrDefaultAsync(inv =>
                                inv.ProductId == item.ProductId &&
                                inv.WarehouseId == request.ToWarehouseId &&
                                inv.StoragePosition == binQty.StoragePosition);

                        if (inventory != null)
                        {
                            inventory.Quantity += binQty.Quantity;
                            inventory.UpdatedAt = DateTime.UtcNow;
                        }
                        else
                        {
                            _context.Inventories.Add(new Inventory
                            {
                                ProductId = item.ProductId,
                                WarehouseId = request.ToWarehouseId,
                                Quantity = binQty.Quantity,
                                StoragePosition = binQty.StoragePosition,
                                UpdatedAt = DateTime.UtcNow
                            });
                        }

                        _context.StockMovements.Add(new StockMovement
                        {
                            ProductId = item.ProductId,
                            WarehouseId = request.ToWarehouseId,
                            QuantityChange = binQty.Quantity,
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

                return Ok(new { Message = $"Nhận hàng {request.TransferNo} thành công.", NewStatus = request.Status });
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                return StatusCode(500, "Lỗi: " + ex.Message);
            }
        }
    }
}