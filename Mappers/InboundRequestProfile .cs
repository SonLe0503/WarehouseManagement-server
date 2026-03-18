using AutoMapper;
using warehouseManagement.DTOs;
using warehouseManagement.DTOs.InboundRequests;
using warehouseManagement.Models;
using WarehouseManagement.DTOs.InboundRequests;

namespace WarehouseManagement.Mappers
{
    public class InboundRequestProfile : Profile
    {
        public InboundRequestProfile()
        {
     
            CreateMap<InboundRequestCreateDto, InboundRequest>();
            CreateMap<InboundRequestItemCreateDto, InboundItem>();

          
            CreateMap<InboundRequest, InboundRequestViewDto>()
                .ForMember(
                    dest => dest.Items,
                    opt => opt.MapFrom(src => src.InboundItems) 
                )
                .ForMember(
                    dest => dest.RejectReason,
                    opt => opt.MapFrom(src =>
                        src.Status == "Rejected" ? "Check logs for details" : null
                    )
                );

            CreateMap<InboundItem, InboundItemDetailDto>()
                .ForMember(
                    dest => dest.Product,
                    opt => opt.MapFrom(src => src.Product) 
                );

            
            CreateMap<Product, ProductDTO>()
                .ForMember(dest => dest.CategoryName,
                    opt => opt.MapFrom(src => src.Category.Name))
                .ForMember(dest => dest.BaseUnitCode,
                    opt => opt.MapFrom(src => src.BaseUnit.Code));
        }
    }
}