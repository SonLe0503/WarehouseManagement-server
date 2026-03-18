using warehouseManagement.DTOs;

namespace warehouseManagement.DTOs.StockTransferRequests
{
    public class StockTransferReceiveDto
    {
        public List<ReceiveTransferItemDto> Items { get; set; } = new();
    }

    public class ReceiveTransferItemDto
    {
        public int StockTransferItemId { get; set; }
        public List<BinQuantityDto> BinQuantities { get; set; } = new();
        public string? LineNote { get; set; }
    }
}
