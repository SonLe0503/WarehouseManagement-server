namespace warehouseManagement.DTOs.OutboundRequests
{
    public class OutboundRequestViewDto
    {
        public int Id { get; set; }
        public string? RequestNo { get; set; }
        public string? CustomerName { get; set; }
        public string? Status { get; set; }
        public DateTime? CreatedAt { get; set; }
        public string? Note { get; set; }
        public int WarehouseId { get; set; }

        public List<OutboundItemDetailDto>? Items { get; set; }

    }
}
