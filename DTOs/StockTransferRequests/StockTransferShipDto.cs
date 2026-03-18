namespace warehouseManagement.DTOs.StockTransferRequests
{
    public class StockTransferShipDto
    {
        public List<ShipTransferItemDto> Items { get; set; } = new();
    }

    public class ShipTransferItemDto
    {
        public int StockTransferItemId { get; set; }
        public decimal PickedQuantity { get; set; }
        public string? StoragePosition { get; set; }
        public string? LineNote { get; set; }
    }
}
