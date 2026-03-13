namespace warehouseManagement.DTOs.StockTransferRequests
{
    public class StockTransferViewDto
    {
        public int Id { get; set; }
        public string TransferNo { get; set; } = null!;
        public int FromWarehouseId { get; set; }
        public string? FromWarehouseName { get; set; }
        public int ToWarehouseId { get; set; }
        public string? ToWarehouseName { get; set; }
        public string? Status { get; set; }
        public string? Note { get; set; }
        public int CreatedBy { get; set; }
        public int? ApprovedBy { get; set; }
        public DateTime? CreatedAt { get; set; }
        public string? RejectReason { get; set; }

        public List<StockTransferItemDetailDto> Items { get; set; } = new();
    }
}
