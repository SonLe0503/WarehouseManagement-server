using warehouseManagement.DTOs;
using warehouseManagement.Models;

namespace warehouseManagement.Services
{
    public interface IUnitConversionService
    {
        Task<IEnumerable<UnitConversion>> GetByProductAsync(int productId);
        Task<UnitConversion> CreateAsync(CreateUnitConversionDTO dto);
        Task<UnitConversion> UpdateAsync(int id, UpdateUnitConversionDTO dto);
        Task<bool> DeactivateAsync(int id);
    }
}
