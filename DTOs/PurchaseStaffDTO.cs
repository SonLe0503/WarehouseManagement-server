using System.ComponentModel.DataAnnotations;

namespace warehouseManagement.DTOs
{
    public class PurchaseStaffDTO
    {

        public class InboundRequestCreateDto
        {
            public string? SupplierName { get; set; }
            public string? Note { get; set; }
            public int WarehouseId { get; set; }
            public List<InboundItemDto> Items { get; set; } = new();
        }

        public class InboundItemDto
        {
            public int ProductId { get; set; }
            public decimal Quantity { get; set; }
            public string? LineNote { get; set; }
        }

        // DTO dùng để hiển thị danh sách cho Purchase Staff
        public class InboundRequestViewDto
        {
            public int Id { get; set; }
            public string? RequestNo { get; set; }
            public string? SupplierName { get; set; }
            public string? Status { get; set; }
            public DateTime? CreatedAt { get; set; }
            public string? Note { get; set; }
            // Thêm thông tin từ ApprovalLog để xem lý do từ chối
            public string? RejectReason { get; set; }
        }


    }
}
