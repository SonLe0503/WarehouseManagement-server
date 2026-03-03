namespace warehouseManagement.DTOs
{
    public class ReceiveInboundRequestDto
    {
        public List<ReceiveInboundItemDto> Items { get; set; } = new();
    }

    public class ReceiveInboundItemDto
    {
        public int InboundItemId { get; set; }
        public decimal ReceivedQuantity { get; set; }
        public string? StoragePosition { get; set; }
        public string? LineNote { get; set; }
    }
}