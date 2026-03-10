using System.Text.Json.Serialization;

namespace warehouseManagement.DTOs
{
    public class StockTransferItemViewDto
    {
        public int Id { get; set; }
        public int ProductId { get; set; }
        public string? ProductName { get; set; }
        public string? ProductSku { get; set; }
        public int UnitId { get; set; }
        public string? UnitName { get; set; }
        public string? UnitCode { get; set; }
        public decimal Quantity { get; set; }
        public decimal? ReceivedQuantity { get; set; }
        public string? FromStoragePosition { get; set; }
        public string? ToStoragePosition { get; set; }
        public string? LineNote { get; set; }
    }

    public class StockTransferRequestViewDto
    {
        public int Id { get; set; }
        public string TransferNo { get; set; } = null!;
        public int FromWarehouseId { get; set; }
        public string? FromWarehouseName { get; set; }
        public int ToWarehouseId { get; set; }
        public string? ToWarehouseName { get; set; }
        public string? Status { get; set; }
        public string? Note { get; set; }
        public int CreatedBy { get; set; }
        public string? CreatedByUsername { get; set; }
        public DateTime? CreatedAt { get; set; }
        public List<StockTransferItemViewDto> StockTransferItems { get; set; } = new();
    }

    public class StockTransferItemCreateDto
    {
        [JsonPropertyName("productId")]
        public int ProductId { get; set; }
        [JsonPropertyName("unitId")]
        public int UnitId { get; set; }
        [JsonPropertyName("quantity")]
        public decimal Quantity { get; set; }
        [JsonPropertyName("fromStoragePosition")]
        public string FromStoragePosition { get; set; } = null!;
        [JsonPropertyName("toStoragePosition")]
        public string ToStoragePosition { get; set; } = null!;
        [JsonPropertyName("lineNote")]
        public string? LineNote { get; set; }
    }

    public class StockTransferRequestCreateDto
    {
        [JsonPropertyName("warehouseId")]
        public int WarehouseId { get; set; }
        [JsonPropertyName("note")]
        public string? Note { get; set; }
        [JsonPropertyName("items")]
        public List<StockTransferItemCreateDto> Items { get; set; } = new();
    }
}