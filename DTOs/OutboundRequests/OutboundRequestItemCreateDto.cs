namespace warehouseManagement.DTOs.OutboundRequests
{
    public class OutboundRequestItemCreateDto
    {
        public int ProductId { get; set; }
        public decimal Quantity { get; set; }
        public int UnitId { get; set; }
        public string? LineNote { get; set; }
    }
}
