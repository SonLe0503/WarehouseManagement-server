namespace warehouseManagement.DTOs.OutboundRequests
{
    public class OutboundRequestCreateDto
    {
        public string? CustomerName { get; set; }
        public string? Note { get; set; }
        public int WarehouseId { get; set; }

        public List<OutboundRequestItemCreateDto> Items { get; set; } = new();
    }
}
