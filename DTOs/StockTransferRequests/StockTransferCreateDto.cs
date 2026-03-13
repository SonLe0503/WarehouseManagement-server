namespace warehouseManagement.DTOs.StockTransferRequests
{
    public class StockTransferCreateDto
    {
        public int FromWarehouseId { get; set; }
        public int ToWarehouseId { get; set; }
        public string? Note { get; set; }

        public List<StockTransferItemCreateDto> Items { get; set; } = new();
    }
}
