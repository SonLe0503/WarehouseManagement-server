using System;
using System.Collections.Generic;

namespace warehouseManagement.Models;

public partial class UnitConversion
{
    public int Id { get; set; }

    public int ProductId { get; set; }

    public int BaseUnitId { get; set; }

    public int FromUnitId { get; set; }

    public decimal ConversionFactor { get; set; }

    public bool IsActive { get; set; }

    public DateTime? CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public virtual Unit BaseUnit { get; set; } = null!;

    public virtual Unit FromUnit { get; set; } = null!;

    public virtual Product Product { get; set; } = null!;
}
