namespace warehouseManagement.Models
{
    public class StockCountSession
    {
        public int Id { get; set; }

        public string CountNo { get; set; }

        public int WarehouseId { get; set; }

        public string Status { get; set; } // Draft / Counting / Completed / Approved

        public string? Note { get; set; }

        public int CreatedBy { get; set; }

        public int? ApprovedBy { get; set; }

        public DateTime CreatedAt { get; set; }

        public DateTime? ApprovedAt { get; set; }

        // Navigation
        public Warehouse Warehouse { get; set; }

        public User CreatedUser { get; set; }

        public User? ApprovedUser { get; set; }

        public ICollection<StockCountItem> Items { get; set; }
    }
}
