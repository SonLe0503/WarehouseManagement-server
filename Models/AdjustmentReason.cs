namespace warehouseManagement.Models
{
    public class AdjustmentReason
    {
        public int Id { get; set; }

        public string Code { get; set; }

        public string Name { get; set; }

        public ICollection<StockCountItem> StockCountItems { get; set; }
    }
}

