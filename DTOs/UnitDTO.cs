namespace warehouseManagement.DTOs
{
    public class UnitDTO
    {
        public string Code { get; set; } = null!;
        public string Name { get; set; } = null!;
        public string? Description { get; set; }
        public bool IsBaseUnit { get; set; }
    }
}
