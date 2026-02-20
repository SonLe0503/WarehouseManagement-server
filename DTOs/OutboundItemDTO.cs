namespace warehouseManagement.DTOs
{
    public class OutboundItemDTO
    {
        public int Id { get; set; }

        public int OutboundRequestId { get; set; }

        public int ProductId { get; set; }

        public decimal Quantity { get; set; }

        public decimal? PickedQuantity { get; set; }

        public string? StoragePosition { get; set; }

        public string? LineNote { get; set; }
    }
}
