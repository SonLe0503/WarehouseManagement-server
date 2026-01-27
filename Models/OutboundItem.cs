using System;
using System.Collections.Generic;

namespace warehouseManagement.Models;

public partial class OutboundItem
{
    public int Id { get; set; }

    public int OutboundRequestId { get; set; }

    public int ProductId { get; set; }

    public decimal Quantity { get; set; }

    public decimal? PickedQuantity { get; set; }

    public string? StoragePosition { get; set; }

    public string? LineNote { get; set; }

    public virtual OutboundRequest OutboundRequest { get; set; } = null!;

    public virtual Product Product { get; set; } = null!;
}
