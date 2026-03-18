using AutoMapper;
using warehouseManagement.DTOs.Sessions;
using warehouseManagement.Models;

namespace warehouseManagement.Mappers
{
    public class StockCountProfile : Profile
    {
        public StockCountProfile()
        {
            CreateMap<StockCountSession, StockCountSessionDTO>();
            CreateMap<StockCountItem, StockCountItemDTO>()
    .ForMember(dest => dest.BaseUnitId,
        opt => opt.MapFrom(src => src.Product.BaseUnitId))
    .ForMember(dest => dest.BaseUnitName,
        opt => opt.MapFrom(src => src.Product.BaseUnit != null ? src.Product.BaseUnit.Name : null));

        }
    }
}
