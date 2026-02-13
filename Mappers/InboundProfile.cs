using AutoMapper;
using warehouseManagement.DTOs;
using warehouseManagement.Models;

namespace warehouseManagement.Mappers
{
    public class InboundProfile : Profile
    {
        public InboundProfile() {
            CreateMap<InboundRequest, InboundRequestDTO>()
                .ForMember(dest => dest.InboundItems, opt => opt.MapFrom(src => src.InboundItems));

            CreateMap<InboundItem, InboundItemDTO>();

        }
    }
}
