using AutoMapper;
using warehouseManagement.DTOs;
using warehouseManagement.Models;

namespace warehouseManagement.Mappers
{
    public class UnitConversionProfile : Profile
    {
        public UnitConversionProfile()
        {
            CreateMap<UnitConversion, UnitConversionDTO>()
    .ForMember(dest => dest.ToUnitId,
        opt => opt.MapFrom(src => src.BaseUnitId))
    .ForMember(dest => dest.Rate,
        opt => opt.MapFrom(src => src.ConversionFactor));
        }
    }
}
