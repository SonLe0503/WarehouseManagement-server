using System.Security.Claims;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using warehouseManagement.DTOs;
using warehouseManagement.Models;

namespace warehouseManagement.Controllers
{

    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "MANAGE,STAFF")]
    public class StockTransferController : ControllerBase
    {
        private readonly WmsContext _context;
        private readonly IMapper _mapper;

        public StockTransferController(WmsContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var transfers = await _context.StockTransferRequests
                .Include(t => t.StockTransferItems).ThenInclude(i => i.Product)
                .Include(t => t.StockTransferItems).ThenInclude(i => i.Unit)
                .Include(t => t.CreatedByNavigation)
                .Include(t => t.FromWarehouse)
                .Include(t => t.ToWarehouse)
                .Where(t => t.FromWarehouseId == t.ToWarehouseId)
                .OrderByDescending(t => t.CreatedAt)
                .ToListAsync();

            return Ok(_mapper.Map<List<BinTransferRequestViewDto>>(transfers));
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var transfer = await _context.StockTransferRequests
                .Include(t => t.StockTransferItems).ThenInclude(i => i.Product)
                .Include(t => t.StockTransferItems).ThenInclude(i => i.Unit)
                .Include(t => t.CreatedByNavigation)
                .Include(t => t.FromWarehouse)
                .Include(t => t.ToWarehouse)
                .FirstOrDefaultAsync(t => t.Id == id);

            if (transfer == null) return NotFound("Không tìm thấy phiếu chuyển bin");
            return Ok(_mapper.Map<BinTransferRequestViewDto>(transfer));
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] BinTransferRequestCreateDto dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            if (dto.Items == null || dto.Items.Count == 0)
                return BadRequest("Phải có ít nhất 1 sản phẩm cần chuyển");

            var userIdStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value
                         ?? User.FindFirst("sub")?.Value
                         ?? User.FindFirst("id")?.Value;
            if (!int.TryParse(userIdStr, out int currentUserId) || currentUserId == 0)
                return Unauthorized("Không xác định được người dùng. Vui lòng đăng nhập lại.");

            var warehouse = await _context.Warehouses.FindAsync(dto.WarehouseId);
            if (warehouse == null) return BadRequest("Kho không tồn tại");

            foreach (var item in dto.Items)
            {
                if (item.FromStoragePosition == item.ToStoragePosition)
                    return BadRequest($"Bin nguồn và bin đích không được trùng nhau (ProductId={item.ProductId})");
                if (item.Quantity <= 0)
                    return BadRequest($"Số lượng phải lớn hơn 0 (ProductId={item.ProductId})");
            }

            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var transferNo = await GenerateTransferNoAsync();
                var transfer = new StockTransferRequest
                {
                    TransferNo = transferNo,
                    FromWarehouseId = dto.WarehouseId,
                    ToWarehouseId = dto.WarehouseId,
                    Status = "Completed",
                    Note = dto.Note,
                    CreatedBy = currentUserId,
                    CreatedAt = DateTime.UtcNow,
                };
                _context.StockTransferRequests.Add(transfer);
                await _context.SaveChangesAsync();

                foreach (var itemDto in dto.Items)
                {
                    decimal baseQty = itemDto.Quantity;
                    var product = await _context.Products.FindAsync(itemDto.ProductId);
                    if (product == null)
                        return BadRequest($"Không tìm thấy sản phẩm Id={itemDto.ProductId}");

                    if (itemDto.UnitId != product.BaseUnitId)
                    {
                        var conversion = await _context.UnitConversions
                            .FirstOrDefaultAsync(c =>
                                c.ProductId == itemDto.ProductId &&
                                c.FromUnitId == itemDto.UnitId &&
                                c.IsActive);
                        if (conversion == null)
                            return BadRequest($"Không tìm thấy quy đổi đơn vị cho sản phẩm Id={itemDto.ProductId}");
                        baseQty = itemDto.Quantity * conversion.ConversionFactor;
                    }

                    var fromInventory = await _context.Inventories
                        .FirstOrDefaultAsync(inv =>
                            inv.ProductId == itemDto.ProductId &&
                            inv.WarehouseId == dto.WarehouseId &&
                            inv.StoragePosition == itemDto.FromStoragePosition);

                    if (fromInventory == null)
                        return BadRequest($"Bin '{itemDto.FromStoragePosition}': không có tồn kho cho sản phẩm Id={itemDto.ProductId}");
                    if (fromInventory.Quantity < baseQty)
                        return BadRequest($"Bin '{itemDto.FromStoragePosition}': không đủ tồn (cần {baseQty}, còn {fromInventory.Quantity})");

                    fromInventory.Quantity -= baseQty;
                    fromInventory.UpdatedAt = DateTime.UtcNow;

                    var toInventory = await _context.Inventories
                        .FirstOrDefaultAsync(inv =>
                            inv.ProductId == itemDto.ProductId &&
                            inv.WarehouseId == dto.WarehouseId &&
                            inv.StoragePosition == itemDto.ToStoragePosition);

                    if (toInventory != null)
                    {
                        toInventory.Quantity += baseQty;
                        toInventory.UpdatedAt = DateTime.UtcNow;
                    }
                    else
                    {
                        _context.Inventories.Add(new Inventory
                        {
                            ProductId = itemDto.ProductId,
                            WarehouseId = dto.WarehouseId,
                            Quantity = baseQty,
                            StoragePosition = itemDto.ToStoragePosition,
                            UpdatedAt = DateTime.UtcNow,
                        });
                    }

                    _context.StockMovements.Add(new StockMovement
                    {
                        ProductId = itemDto.ProductId,
                        WarehouseId = dto.WarehouseId,
                        QuantityChange = -baseQty,
                        StoragePosition = itemDto.FromStoragePosition,
                        RefType = "StockTransfer",
                        RefId = transfer.Id,
                        CreatedAt = DateTime.UtcNow,
                    });
                    _context.StockMovements.Add(new StockMovement
                    {
                        ProductId = itemDto.ProductId,
                        WarehouseId = dto.WarehouseId,
                        QuantityChange = baseQty,
                        StoragePosition = itemDto.ToStoragePosition,
                        RefType = "StockTransfer",
                        RefId = transfer.Id,
                        CreatedAt = DateTime.UtcNow,
                    });

                    _context.StockTransferItems.Add(new StockTransferItem
                    {
                        StockTransferRequestId = transfer.Id,
                        ProductId = itemDto.ProductId,
                        UnitId = itemDto.UnitId,
                        Quantity = itemDto.Quantity,
                        ReceivedQuantity = baseQty,
                        FromStoragePosition = itemDto.FromStoragePosition,
                        ToStoragePosition = itemDto.ToStoragePosition,
                        LineNote = itemDto.LineNote,
                    });
                }

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                return Ok(new
                {
                    Message = "Chuyển bin thành công",
                    TransferNo = transferNo,
                    TransferId = transfer.Id,
                });
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                return StatusCode(500, "Lỗi khi xử lý chuyển bin: " + ex.Message);
            }
        }

        private async Task<string> GenerateTransferNoAsync()
        {
            var today = DateTime.UtcNow.ToString("yyyyMMdd");
            var prefix = $"STR-{today}-";
            var lastNo = await _context.StockTransferRequests
                .Where(t => t.TransferNo.StartsWith(prefix))
                .OrderByDescending(t => t.TransferNo)
                .Select(t => t.TransferNo)
                .FirstOrDefaultAsync();

            int seq = 1;
            if (lastNo != null)
            {
                var parts = lastNo.Split('-');
                if (parts.Length == 3 && int.TryParse(parts[2], out int last))
                    seq = last + 1;
            }
            return $"{prefix}{seq:D3}";
        }
    }
}