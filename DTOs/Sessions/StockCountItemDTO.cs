namespace warehouseManagement.DTOs.Sessions
{
    public class StockCountItemDTO
    {
        public int Id { get; set; }

        public int ProductId { get; set; }

        public string? StoragePosition { get; set; }

        public decimal SystemQuantity { get; set; }

        public decimal? ActualQuantity { get; set; }

        public decimal? Difference { get; set; }

        public int? ReasonId { get; set; }

        public string? Note { get; set; }
    }
}
