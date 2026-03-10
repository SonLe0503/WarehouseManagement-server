namespace warehouseManagement.Models;

public partial class Bin
{
    public int Id { get; set; }
    public string Code { get; set; } = null!;
    public string? Name { get; set; }
    public int WarehouseId { get; set; }
    public string Status { get; set; } = "Available"; 
    public DateTime? CreatedAt { get; set; }

    public virtual Warehouse Warehouse { get; set; } = null!;
}