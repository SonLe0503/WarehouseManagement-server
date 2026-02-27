namespace warehouseManagement.DTOs.InboundRequests
{
    public class InboundItemDetailDto
    {
        public int Id { get; set; }
        public int ProductId { get; set; }
        public decimal Quantity { get; set; }
        public int UnitId { get; set; }
        public UnitDTO? Unit { get; set; }
        public string? LineNote { get; set; }
        public ProductDTO? Product { get; set; }  
    }



}
