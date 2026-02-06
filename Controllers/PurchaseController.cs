using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using warehouseManagement.DTOs;
using warehouseManagement.Models;

namespace warehouseManagement.Controllers
{
    public class PurchaseController : Controller
    {

        private readonly WmsContext _context;
        private readonly IConfiguration _config;
        private readonly IMapper _mapper;
        public PurchaseController(WmsContext context, IConfiguration config , IMapper mapper)
        {
            _context = context;
            _config = config;
            _mapper = mapper;
        }
        public IActionResult Index()
        {
            return View();

        }

        [HttpPost("create")]
        public async Task<IActionResult> CreateOrder([FromBody] InBoundRequestDTOs dto) 
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var request = _mapper.Map<InboundRequest>(dto);
                request.RequestNo = "PUR-" + DateTime.Now.Ticks;
                request.Status = "Pending";
                request.CreatedAt = DateTime.Now;
                request.CreatedBy = 1;

                _context.InboundRequests.Add(request);
                await _context.SaveChangesAsync();

                await transaction.CommitAsync(); 
                return Ok(new { message = "Tạo đơn mua thành công", id = request.Id });
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                return StatusCode(500, "Lỗi hệ thống: " + ex.Message);
            }
        }

        [HttpPost("update/{id}")]
        public async Task<IActionResult> UpdateInBound(int id, [FromBody] InBoundRequestDTOs dto)
        {
            var existingRequest = await _context.InboundRequests
                .Include(r => r.InboundItems)
                .FirstOrDefaultAsync(r => r.Id == id);

            if (existingRequest == null) return NotFound("Không tìm thấy đơn hàng");

            if (existingRequest.Status != "Pending")
                return BadRequest("Không thể sửa đơn đã được duyệt hoặc xử lý.");

          
            _mapper.Map(dto, existingRequest);

           
            _context.InboundItems.RemoveRange(existingRequest.InboundItems);
            existingRequest.InboundItems = _mapper.Map<List<InboundItem>>(dto.Items);

            await _context.SaveChangesAsync();
            return Ok("Cập nhật thành công");
        }

        [HttpPost("delete/{id}")]
        public async Task<IActionResult> DeleteInBound(int id)
        {
            var request = await _context.InboundRequests.FindAsync(id);

            if (request == null) return NotFound("Không tìm thấy đơn hàng");

          
            if (request.Status != "Pending")
                return BadRequest("Chỉ có thể xóa đơn hàng ở trạng thái Pending.");

            _context.InboundRequests.Remove(request);
            await _context.SaveChangesAsync();

            return Ok("Đã xóa đơn hàng thành công");
        }
    }
}
