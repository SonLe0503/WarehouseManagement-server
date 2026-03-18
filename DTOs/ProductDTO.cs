namespace warehouseManagement.DTOs
{
    public class CreateProductDTO
    {
        public string Sku { get; set; } = null!;
        public string Name { get; set; } = null!;
        public int CategoryId { get; set; }
        public int BaseUnitId { get; set; }
    }
    public class UpdateProductDTO
    {
        public string? Name { get; set; }
        public int? CategoryId { get; set; }
        public int? BaseUnitId { get; set; }
        public string? Status { get; set; }
    }
    public class ProductDTO
    {
        public int Id { get; set; }
        public string Sku { get; set; } = null!;
        public string Name { get; set; } = null!;
        public string CategoryName { get; set; } = null!;
        public int BaseUnitId { get; set; }
        public string BaseUnitCode { get; set; } = null!;
        public string? Status { get; set; }
        public DateTime? CreatedAt { get; set; }
    }

}