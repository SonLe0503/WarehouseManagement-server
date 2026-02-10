using warehouseManagement.Models;

namespace warehouseManagement.DTOs
{
    public class OutboundRequestDTO
    {
        public int Id { get; set; }

        public string? RequestNo { get; set; }

        public string? CustomerName { get; set; }

        public string? Status { get; set; }

        public string? Note { get; set; }

        public int WarehouseId { get; set; }

        public int CreatedBy { get; set; }

        public int? ApprovedBy { get; set; }

        public DateTime? CreatedAt { get; set; }

        public virtual ICollection<OutboundItem> OutboundItems { get; set; } = new List<OutboundItem>();


    }
}
