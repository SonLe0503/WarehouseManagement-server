namespace warehouseManagement.DTOs
{
    public class InventoryViewDto
    {
        public int Id { get; set; }
        public int ProductId { get; set; }
        public string? ProductName { get; set; }
        public string? Sku { get; set; }
        public int WarehouseId { get; set; }
        public string? WarehouseName { get; set; }
        public string? WarehouseCode { get; set; }
        public decimal Quantity { get; set; }
        public string? StoragePosition { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }
}
