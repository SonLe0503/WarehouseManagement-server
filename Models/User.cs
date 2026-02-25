using System;
using System.Collections.Generic;

namespace warehouseManagement.Models;

public partial class User
{
    public int Id { get; set; }

    public string Username { get; set; } = null!;

    public string? Email { get; set; }

    public string PasswordHash { get; set; } = null!;

    public string Status { get; set; } = null!;

    public DateTime? CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public int? WarehouseId { get; set; }

    public virtual ICollection<ApprovalLog> ApprovalLogs { get; set; } = new List<ApprovalLog>();

    public virtual ICollection<InboundRequest> InboundRequestApprovedByNavigations { get; set; } = new List<InboundRequest>();

    public virtual ICollection<InboundRequest> InboundRequestCreatedByNavigations { get; set; } = new List<InboundRequest>();

    public virtual ICollection<OutboundRequest> OutboundRequestApprovedByNavigations { get; set; } = new List<OutboundRequest>();

    public virtual ICollection<OutboundRequest> OutboundRequestCreatedByNavigations { get; set; } = new List<OutboundRequest>();

    public virtual ICollection<StockTransferRequest> StockTransferRequestApprovedByNavigations { get; set; } = new List<StockTransferRequest>();

    public virtual ICollection<StockTransferRequest> StockTransferRequestCreatedByNavigations { get; set; } = new List<StockTransferRequest>();

    public virtual Warehouse? Warehouse { get; set; }

    public virtual ICollection<Role> Roles { get; set; } = new List<Role>();
}
