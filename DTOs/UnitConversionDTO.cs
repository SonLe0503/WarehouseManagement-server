namespace warehouseManagement.DTOs
{
    public class CreateUnitConversionDTO
    {
        public int ProductId { get; set; }
        public int FromUnitId { get; set; }
        public decimal ConversionFactor { get; set; }
    }
    public class UpdateUnitConversionDTO
    {
        public decimal ConversionFactor { get; set; }
    }
    public class UnitConversionDTO
    {
        public int Id { get; set; }
        public int ProductId { get; set; }
        public int FromUnitId { get; set; }
        public int ToUnitId { get; set; }
        public decimal Rate { get; set; }
    }
}
