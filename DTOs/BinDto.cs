namespace warehouseManagement.DTOs
{
    public class BinViewDto
    {
        public int Id { get; set; }
        public string Code { get; set; } = null!;
        public string? Name { get; set; }
        public int WarehouseId { get; set; }
        public string? WarehouseName { get; set; }
        public string Status { get; set; } = null!;
        public DateTime? CreatedAt { get; set; }
    }

    public class BinCreateDto
    {
        public string Code { get; set; } = null!;
        public string? Name { get; set; }
        public int WarehouseId { get; set; }
    }

    public class BinUpdateDto
    {
        public string Code { get; set; } = null!;
        public string? Name { get; set; }
        public string Status { get; set; } = null!;
    }
}