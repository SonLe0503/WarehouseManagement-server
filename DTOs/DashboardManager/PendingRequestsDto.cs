namespace warehouseManagement.DTOs.DashboardManager
{
    public class PendingRequestsDto
    {
        public int PendingInboundRequests { get; set; }
        public int PendingOutboundRequests { get; set; }
        public int PendingTransferInRequests { get; set; }
        public int PendingTransferOutRequests { get; set; }
        public int TotalPendingRequests { get; set; }
    }
}
