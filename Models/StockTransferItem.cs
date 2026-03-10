using System;
using System.Collections.Generic;

namespace warehouseManagement.Models;

public partial class StockTransferItem
{
    public int Id { get; set; }

    public int StockTransferRequestId { get; set; }

    public int ProductId { get; set; }

    public decimal Quantity { get; set; }

    public decimal? ReceivedQuantity { get; set; }

    public string? FromStoragePosition { get; set; }

    public string? ToStoragePosition { get; set; }
    public int UnitId { get; set; }
    public virtual Unit Unit { get; set; } = null!;

    public string? LineNote { get; set; }

    public virtual Product Product { get; set; } = null!;

    public virtual StockTransferRequest StockTransferRequest { get; set; } = null!;
}
