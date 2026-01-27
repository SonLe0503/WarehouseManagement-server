using System;
using System.Collections.Generic;

namespace warehouseManagement.Models;

public partial class InboundRequest
{
    public int Id { get; set; }

    public string? RequestNo { get; set; }

    public string? SupplierName { get; set; }

    public string? Status { get; set; }

    public string? Note { get; set; }

    public int WarehouseId { get; set; }

    public int CreatedBy { get; set; }

    public int? ApprovedBy { get; set; }

    public DateTime? ApprovedAt { get; set; }

    public DateTime? CreatedAt { get; set; }

    public virtual User? ApprovedByNavigation { get; set; }

    public virtual User CreatedByNavigation { get; set; } = null!;

    public virtual ICollection<InboundItem> InboundItems { get; set; } = new List<InboundItem>();

    public virtual Warehouse Warehouse { get; set; } = null!;
}
