using System;
using System.Collections.Generic;

namespace warehouseManagement.Models;

public partial class Warehouse
{
    public int Id { get; set; }

    public string Code { get; set; } = null!;

    public string Name { get; set; } = null!;

    public string? Address { get; set; }

    public string? Status { get; set; }

    public DateTime? CreatedAt { get; set; }

    public virtual ICollection<InboundRequest> InboundRequests { get; set; } = new List<InboundRequest>();

    public virtual ICollection<Inventory> Inventories { get; set; } = new List<Inventory>();

    public virtual ICollection<OutboundRequest> OutboundRequests { get; set; } = new List<OutboundRequest>();

    public virtual ICollection<StockMovement> StockMovements { get; set; } = new List<StockMovement>();

    public virtual ICollection<StockTransferRequest> StockTransferRequestFromWarehouses { get; set; } = new List<StockTransferRequest>();

    public virtual ICollection<StockTransferRequest> StockTransferRequestToWarehouses { get; set; } = new List<StockTransferRequest>();
}
