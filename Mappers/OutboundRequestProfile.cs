using AutoMapper;
using warehouseManagement.DTOs.OutboundRequests;
using warehouseManagement.Models;

namespace warehouseManagement.Mappers
{
    public class OutboundRequestProfile : Profile
    {
        public OutboundRequestProfile() 
        {
            CreateMap<OutboundRequestCreateDto, OutboundRequest>();

            CreateMap<OutboundItem, OutboundItemDetailDto>();

            CreateMap<OutboundRequest, OutboundRequestViewDto>()
                .ForMember(dest => dest.Items,
                    opt => opt.MapFrom(src => src.OutboundItems));
        }
    }
}
