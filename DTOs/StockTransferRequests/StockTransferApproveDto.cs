namespace warehouseManagement.DTOs.StockTransferRequests
{
    public class StockTransferApproveDto
    {
        public string Action { get; set; } = null!; // "Approve" hoặc "Reject"
        public string? Comment { get; set; }
        public string? RejectReason { get; set; }
    }
}
