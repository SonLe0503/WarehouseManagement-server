namespace warehouseManagement.Models
{
    public class SalesOrderItem
    {
        public int Id { get; set; }
        public int SalesOrderId { get; set; }
        public int ProductId { get; set; }
        public decimal Quantity { get; set; }

        public SalesOrder? SalesOrder { get; set; }
        public Product? Product { get; set; }
    }
}
