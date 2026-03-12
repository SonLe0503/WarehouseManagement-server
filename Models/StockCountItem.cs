namespace warehouseManagement.Models
{
    public class StockCountItem
    {
        public int Id { get; set; }

        public int StockCountSessionId { get; set; }

        public int ProductId { get; set; }

        public string? StoragePosition { get; set; }

        public decimal SystemQuantity { get; set; }

        public decimal? ActualQuantity { get; set; }

        public decimal? Difference { get; set; }

        public int? ReasonId { get; set; }

        public string? Note { get; set; }

        // Navigation
        public StockCountSession Session { get; set; }

        public Product Product { get; set; }

        public AdjustmentReason? Reason { get; set; }
    }
}
