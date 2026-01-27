using System;
using System.Collections.Generic;

namespace warehouseManagement.Models;

public partial class Product
{
    public int Id { get; set; }

    public int CategoryId { get; set; }

    public string Sku { get; set; } = null!;

    public string Name { get; set; } = null!;

    public int BaseUnitId { get; set; }

    public string? Status { get; set; }

    public DateTime? CreatedAt { get; set; }

    public virtual Unit BaseUnit { get; set; } = null!;

    public virtual Category Category { get; set; } = null!;

    public virtual ICollection<InboundItem> InboundItems { get; set; } = new List<InboundItem>();

    public virtual ICollection<Inventory> Inventories { get; set; } = new List<Inventory>();

    public virtual ICollection<OutboundItem> OutboundItems { get; set; } = new List<OutboundItem>();

    public virtual ICollection<StockMovement> StockMovements { get; set; } = new List<StockMovement>();

    public virtual ICollection<StockTransferItem> StockTransferItems { get; set; } = new List<StockTransferItem>();

    public virtual ICollection<UnitConversion> UnitConversions { get; set; } = new List<UnitConversion>();
}
