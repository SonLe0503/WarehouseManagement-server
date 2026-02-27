using System;
using System.Collections.Generic;

namespace warehouseManagement.Models;

public partial class InboundItem
{
    public int Id { get; set; }

    public int InboundRequestId { get; set; }

    public int ProductId { get; set; }

    public decimal Quantity { get; set; }

    public decimal? ReceivedQuantity { get; set; }

    public string? StoragePosition { get; set; }

    public string? LineNote { get; set; }

    public int UnitId { get; set; }

    public virtual InboundRequest InboundRequest { get; set; } = null!;

    public virtual Product Product { get; set; } = null!;

    public virtual Unit Unit { get; set; } = null!;
}
