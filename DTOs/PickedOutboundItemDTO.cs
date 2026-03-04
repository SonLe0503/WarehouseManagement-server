namespace warehouseManagement.DTOs
{
    public class PickedInboundRequestDTO
    {
        public List<PickedInboundItemDTO> Items { get; set; } = new();
    }

    public class PickedInboundItemDTO
    {
        public int OutboundItemId { get; set; }
        public decimal PickedQuantity { get; set; }
        public string? StoragePosition { get; set; }
        public string? LineNote { get; set; }
    }
}
