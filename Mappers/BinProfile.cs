using AutoMapper;
using warehouseManagement.DTOs;
using warehouseManagement.Models;

namespace warehouseManagement.Mappers
{
    public class BinProfile : Profile
    {
        public BinProfile()
        {
            CreateMap<Bin, BinViewDto>()
                .ForMember(dest => dest.WarehouseName,
                    opt => opt.MapFrom(src => src.Warehouse.Name));
            CreateMap<BinCreateDto, Bin>();
        }
    }
}