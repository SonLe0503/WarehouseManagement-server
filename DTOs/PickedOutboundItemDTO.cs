namespace warehouseManagement.DTOs
{
    public class PickedInboundRequestDTO
    {
        public List<PickedOutboundItemDTO> Items { get; set; } = new();
    }

    public class PickedOutboundItemDTO
    {
        public int OutboundItemId { get; set; }
        public List<ShipBinQuantityDto> BinQuantities { get; set; } = new();
        public string? LineNote { get; set; }
    }

    public class ShipBinQuantityDto
    {
        public string StoragePosition { get; set; } = null!;
        public int UnitId { get; set; }
        public decimal Quantity { get; set; }
    }
}