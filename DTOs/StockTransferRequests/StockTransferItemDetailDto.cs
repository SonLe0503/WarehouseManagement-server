using warehouseManagement.DTOs;

namespace warehouseManagement.DTOs.StockTransferRequests
{
    public class StockTransferItemDetailDto
    {
        public int Id { get; set; }
        public int ProductId { get; set; }
        public decimal Quantity { get; set; }
        public decimal? ReceivedQuantity { get; set; }
        public string? FromStoragePosition { get; set; }
        public string? ToStoragePosition { get; set; }
        public string? LineNote { get; set; }

        public ProductDTO? Product { get; set; }
    }
}
