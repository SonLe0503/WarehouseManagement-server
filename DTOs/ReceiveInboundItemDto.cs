namespace warehouseManagement.DTOs
{
    public class ReceiveInboundRequestDto
    {
        public List<ReceiveInboundItemDto> Items { get; set; } = new();
    }

    public class ReceiveInboundItemDto
    {
        public int InboundItemId { get; set; }
        public List<BinQuantityDto> BinQuantities { get; set; } = new();
        public string? LineNote { get; set; }
    }

    public class BinQuantityDto
    {
        public string StoragePosition { get; set; } = null!;
        public decimal Quantity { get; set; }
    }
}