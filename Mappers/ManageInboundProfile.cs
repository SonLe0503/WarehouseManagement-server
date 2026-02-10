using AutoMapper;
using warehouseManagement.DTOs;
using warehouseManagement.Models;

namespace warehouseManagement.Mappers
{
    public class ManageInboundProfile : Profile
    {
        public ManageInboundProfile() {
            CreateMap<InboundRequest, InboundRequestDTO>()
                .ForMember(dest => dest.InboundItems, opt => opt.MapFrom(src => src.InboundItems));

            CreateMap<InboundItem, InboundItemDTO>();

        }
    }
}
