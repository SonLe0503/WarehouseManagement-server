namespace warehouseManagement.DTOs
{
    public class InboundItemDTO
    {
        public int Id { get; set; }
        public int InboundRequestId { get; set; }
        public int ProductId { get; set; }
        public decimal Quantity { get; set; }
        public decimal? ReceivedQuantity { get; set; }
        public string? StoragePosition { get; set; }
        public string? LineNote { get; set; }
        public int UnitId { get; set; }
    }

}
