using System;
using System.Collections.Generic;

namespace warehouseManagement.Models;

public partial class StockTransferRequest
{
    public int Id { get; set; }

    public string TransferNo { get; set; } = null!;

    public int FromWarehouseId { get; set; }

    public int ToWarehouseId { get; set; }

    public string? Status { get; set; }

    public string? Note { get; set; }

    public int CreatedBy { get; set; }

    public int? ApprovedBy { get; set; }

    public DateTime? CreatedAt { get; set; }

    public virtual User? ApprovedByNavigation { get; set; }

    public virtual User CreatedByNavigation { get; set; } = null!;

    public virtual Warehouse FromWarehouse { get; set; } = null!;

    public virtual ICollection<StockTransferItem> StockTransferItems { get; set; } = new List<StockTransferItem>();

    public virtual Warehouse ToWarehouse { get; set; } = null!;
}
