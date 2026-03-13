namespace warehouseManagement.DTOs.StockTransferRequests
{
   
        public class StockTransferItemCreateDto
        {
            public int ProductId { get; set; }
            public int UnitId { get; set; }
            public decimal Quantity { get; set; }
            public string? LineNote { get; set; }
        }
    
}
