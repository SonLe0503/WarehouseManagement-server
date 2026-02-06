using warehouseManagement.Models;

namespace warehouseManagement.DTOs
{
    public class InboundRequestDTOs
    {
        public int Id { get; set; }

        public string? SupplierName { get; set; }

        public string? Note { get; set; }

        public int WarehouseId { get; set; }

        public int CreatedBy { get; set; }

        public virtual Warehouse Warehouse { get; set; } = null!;
    }
}
