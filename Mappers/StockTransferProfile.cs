using AutoMapper;
using warehouseManagement.DTOs;
using warehouseManagement.DTOs.StockTransferRequests;
using warehouseManagement.Models;

namespace warehouseManagement.Mappers
{
    public class StockTransferProfile : Profile
    {
        public StockTransferProfile()
        {
            CreateMap<StockTransferCreateDto, StockTransferRequest>();

            CreateMap<StockTransferRequest, StockTransferViewDto>()
                .ForMember(dest => dest.Items,
                    opt => opt.MapFrom(src => src.StockTransferItems))
                .ForMember(dest => dest.FromWarehouseName,
                    opt => opt.MapFrom(src => src.FromWarehouse.Name))
                .ForMember(dest => dest.ToWarehouseName,
                    opt => opt.MapFrom(src => src.ToWarehouse.Name))
                .ForMember(dest => dest.RejectReason,
                    opt => opt.MapFrom(src =>
                        src.Status == "Rejected" ? "Check logs for details" : null));

            CreateMap<StockTransferItem, StockTransferItemDetailDto>()
                .ForMember(dest => dest.Product,
                    opt => opt.MapFrom(src => src.Product));
        }
    }
}
