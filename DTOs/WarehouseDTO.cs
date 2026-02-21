namespace warehouseManagement.DTOs
{
    public class CreateWarehouseDTO
    {
        public string Code { get; set; } = null!;
        public string Name { get; set; } = null!;
        public string? Address { get; set; }
    }

    public class UpdateWarehouseDTO
    {
        public string Code { get; set; } = null!;
        public string Name { get; set; } = null!;
        public string? Address { get; set; }
        public string? Status { get; set; }
    }

    public class WarehouseDTO
    {
        public int Id { get; set; }
        public string Code { get; set; } = null!;
        public string Name { get; set; } = null!;
        public string? Address { get; set; }
        public string? Status { get; set; }
        public DateTime? CreatedAt { get; set; }
    }
}
