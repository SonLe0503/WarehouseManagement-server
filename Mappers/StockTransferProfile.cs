using AutoMapper;
using warehouseManagement.DTOs;
using warehouseManagement.Models;

namespace warehouseManagement.Mappers
{
    public class StockTransferProfile : Profile
    {
        public StockTransferProfile()
        {
            CreateMap<StockTransferItem, StockTransferItemViewDto>()
                .ForMember(dest => dest.ProductName,
                    opt => opt.MapFrom(src => src.Product.Name))
                .ForMember(dest => dest.ProductSku,
                    opt => opt.MapFrom(src => src.Product.Sku))
                .ForMember(dest => dest.UnitName,
                    opt => opt.MapFrom(src => src.Unit.Name))
                .ForMember(dest => dest.UnitCode,
                    opt => opt.MapFrom(src => src.Unit.Code));

            CreateMap<StockTransferRequest, StockTransferRequestViewDto>()
                .ForMember(dest => dest.FromWarehouseName,
                    opt => opt.MapFrom(src => src.FromWarehouse.Name))
                .ForMember(dest => dest.ToWarehouseName,
                    opt => opt.MapFrom(src => src.ToWarehouse.Name))
                .ForMember(dest => dest.CreatedByUsername,
                    opt => opt.MapFrom(src => src.CreatedByNavigation != null ? src.CreatedByNavigation.Username : null));
        }
    }
}