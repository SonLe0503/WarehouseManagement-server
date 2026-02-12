using warehouseManagement.Models;

namespace warehouseManagement.DTOs
{
    public class InboundRequestDTO
    {
        public int Id { get; set; }
        public string? RequestNo { get; set; }
        public string? SupplierName { get; set; }
        public string? Status { get; set; }
        public string? Note { get; set; }
        public int WarehouseId { get; set; }
        public int CreatedBy { get; set; }
        public int? ApprovedBy { get; set; }
        public DateTime? ApprovedAt { get; set; }
        public DateTime? CreatedAt { get; set; }
        public List<InboundItemDTO> InboundItems { get; set; } = new List<InboundItemDTO>();


    }
    public class ApproveInboundRequestDTO
    {
        public string Action { get; set; } = null!;         // "Approve" hoặc "Reject"
        //public string? Comment { get; set; }
        //public string? RejectReason { get; set; }           // Chỉ dùng khi Action = "Reject"
    }
}
