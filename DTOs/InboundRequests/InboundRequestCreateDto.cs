namespace WarehouseManagement.DTOs.InboundRequests
{
    public class InboundRequestCreateDto
    {
        public string? SupplierName { get; set; }
        public string? Note { get; set; }
        public int WarehouseId { get; set; }

        public List<InboundRequestItemCreateDto> Items { get; set; } = new();
    }
}
