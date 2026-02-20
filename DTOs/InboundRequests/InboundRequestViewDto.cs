using warehouseManagement.DTOs.InboundRequests;

namespace WarehouseManagement.DTOs.InboundRequests
{
    public class InboundRequestViewDto
    {
        public int Id { get; set; }
        public string? RequestNo { get; set; }
        public string? SupplierName { get; set; }
        public string? Status { get; set; }
        public DateTime CreatedAt { get; set; }
        public string? Note { get; set; }
        public string? RejectReason { get; set; }
        public int WarehouseId { get; set; }  
        public List<InboundItemDetailDto>? Items { get; set; }
    }
}
