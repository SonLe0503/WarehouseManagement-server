using System;
using System.Collections.Generic;

namespace warehouseManagement.Models;

public partial class StockMovement
{
    public int Id { get; set; }

    public int ProductId { get; set; }

    public int WarehouseId { get; set; }

    public decimal QuantityChange { get; set; }

    public string? StoragePosition { get; set; }

    public string? RefType { get; set; }

    public int? RefId { get; set; }

    public DateTime? CreatedAt { get; set; }

    public virtual Product Product { get; set; } = null!;

    public virtual Warehouse Warehouse { get; set; } = null!;
}
