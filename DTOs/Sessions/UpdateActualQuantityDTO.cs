namespace warehouseManagement.DTOs.Sessions
{
    public class UpdateActualQuantityDTO
    {
        public decimal ActualQuantity { get; set; }

        public int? ReasonId { get; set; }

        public string? Note { get; set; }
    }
}
