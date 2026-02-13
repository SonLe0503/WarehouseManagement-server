using AutoMapper;
using warehouseManagement.DTOs;
using warehouseManagement.Models;

namespace warehouseManagement.Mappers
{
    public class OutboundProfile : Profile
    {
        public OutboundProfile()
        {
            CreateMap<OutboundRequest, OutboundRequestDTO>()
                .ForMember(dest => dest.OutboundItems, opt => opt.MapFrom(src => src.OutboundItems));

            CreateMap<OutboundItem, OutboundItemDTO>();

        }
    }
}
