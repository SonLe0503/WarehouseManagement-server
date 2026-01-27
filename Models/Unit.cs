using System;
using System.Collections.Generic;

namespace warehouseManagement.Models;

public partial class Unit
{
    public int Id { get; set; }

    public string Code { get; set; } = null!;

    public string Name { get; set; } = null!;

    public string? Description { get; set; }

    public bool IsBaseUnit { get; set; }

    public DateTime? CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public virtual ICollection<Product> Products { get; set; } = new List<Product>();

    public virtual ICollection<UnitConversion> UnitConversionBaseUnits { get; set; } = new List<UnitConversion>();

    public virtual ICollection<UnitConversion> UnitConversionFromUnits { get; set; } = new List<UnitConversion>();
}
