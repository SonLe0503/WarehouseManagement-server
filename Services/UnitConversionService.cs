using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using warehouseManagement.DTOs;
using warehouseManagement.Models;

namespace warehouseManagement.Services
{
    public class UnitConversionService : IUnitConversionService
    {
        private readonly WmsContext _context;
        public UnitConversionService(WmsContext context) 
        { 
            _context = context;
        }
        public async Task<IEnumerable<UnitConversion>> GetByProductAsync(int productId)
        {
            return await _context.UnitConversions
                .Where(x => x.ProductId == productId && x.IsActive)
                .ToListAsync();
        }
        public async Task<UnitConversion> CreateAsync(CreateUnitConversionDTO dto)
        {
            var product = await _context.Products
                .FirstOrDefaultAsync(p => p.Id == dto.ProductId);

            if (product == null)
                throw new Exception("Product not found");

            if (product.BaseUnitId == dto.FromUnitId)
                throw new Exception("Cannot create conversion for base unit");

            if (dto.ConversionFactor <= 0)
                throw new Exception("Conversion factor must be greater than 0");

            var existing = await _context.UnitConversions
                .FirstOrDefaultAsync(x =>
                    x.ProductId == dto.ProductId &&
                    x.FromUnitId == dto.FromUnitId);

            if (existing != null)
            {
                if (existing.IsActive)
                    throw new Exception("Conversion already exists");

                // Reactivate nếu đã từng tồn tại
                existing.IsActive = true;
                existing.ConversionFactor = dto.ConversionFactor;
                existing.UpdatedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync();
                return existing;
            }

            var entity = new UnitConversion
            {
                ProductId = dto.ProductId,
                BaseUnitId = product.BaseUnitId,
                FromUnitId = dto.FromUnitId,
                ConversionFactor = dto.ConversionFactor,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };

            _context.UnitConversions.Add(entity);
            await _context.SaveChangesAsync();

            return entity;
        }

        public async Task<UnitConversion> UpdateAsync(int id, UpdateUnitConversionDTO dto)
        {
            var entity = await _context.UnitConversions
                .FirstOrDefaultAsync(x => x.Id == id && x.IsActive);

            if (entity == null)
                throw new Exception("Conversion not found");

            if (dto.ConversionFactor <= 0)
                throw new Exception("Invalid factor");

            entity.ConversionFactor = dto.ConversionFactor;
            entity.UpdatedAt = DateTime.UtcNow;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException ex)
            {
                if (ex.InnerException is SqlException sqlEx &&
                    sqlEx.Number == 2627)
                {
                    throw new Exception("Conversion already exists.");
                }

                throw;
            }

            return entity;
        }
        public async Task<bool> DeactivateAsync(int id)
        {
            var entity = await _context.UnitConversions
                .FirstOrDefaultAsync(x => x.Id == id);

            if (entity == null)
                return false;

            entity.IsActive = false;
            entity.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return true;
        }
    }
}
