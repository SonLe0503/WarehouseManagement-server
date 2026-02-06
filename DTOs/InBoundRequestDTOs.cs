using warehouseManagement.Models;

namespace warehouseManagement.DTOs
{
    public class InBoundRequestDTOs
    {

        public string SupplierName { get; set; }
        public int WarehouseId { get; set; }
        public string? Note { get; set; }
        public List<InBoundIteamDtos> Items { get; set; } = new();

    }
}
