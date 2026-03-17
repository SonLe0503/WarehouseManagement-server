namespace warehouseManagement.DTOs.Sessions
{
    public class StockCountSessionDTO
    {
        public int Id { get; set; }

        public string CountNo { get; set; }

        public int WarehouseId { get; set; }

        public string Note { get; set; }

        public string Status { get; set; }

        public DateTime CreatedAt { get; set; }
    }
}
