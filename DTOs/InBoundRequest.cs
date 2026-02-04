namespace warehouseManagement.DTOs
{
    public class InBoundRequest
    {
        public int InBoundRequestId { get; set; }
        public  string SupplierName { get; set; }
        public int InBoundWarehouseId { get; set; }
        public DateTime CreateAt { get; set; }
        public int CreatedBy { get; set; }


    }
}
