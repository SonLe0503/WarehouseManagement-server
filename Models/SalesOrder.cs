using System;

namespace warehouseManagement.Models
{
    public class SalesOrder
    {
        public int Id { get; set; }
        public string? OrderNo { get; set; }
        public string? CustomerName { get; set; }
        public string? Status { get; set; }
        public string? Note { get; set; }
        public int CreatedBy { get; set; }
        public DateTime? CreatedAt { get; set; }

        public User? Creator { get; set; }
        public ICollection<SalesOrderItem> Items { get; set; }
    }

}
