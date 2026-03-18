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
            // ─── Bin-to-bin (cùng kho) ────────────────────────────────────────────
            CreateMap<StockTransferItem, BinTransferItemViewDto>()
                .ForMember(dest => dest.ProductName, opt => opt.MapFrom(src => src.Product.Name))
                .ForMember(dest => dest.ProductSku, opt => opt.MapFrom(src => src.Product.Sku))
                .ForMember(dest => dest.UnitName, opt => opt.MapFrom(src => src.Unit.Name))
                .ForMember(dest => dest.UnitCode, opt => opt.MapFrom(src => src.Unit.Code));

            CreateMap<StockTransferRequest, BinTransferRequestViewDto>()
                .ForMember(dest => dest.FromWarehouseName, opt => opt.MapFrom(src => src.FromWarehouse.Name))
                .ForMember(dest => dest.ToWarehouseName, opt => opt.MapFrom(src => src.ToWarehouse.Name))
                .ForMember(dest => dest.CreatedByUsername,
                    opt => opt.MapFrom(src => src.CreatedByNavigation != null ? src.CreatedByNavigation.Username : null));

            // ─── Cross-warehouse (khác kho) ───────────────────────────────────────
            CreateMap<StockTransferItem, StockTransferItemDetailDto>()
                .ForMember(dest => dest.Product, opt => opt.MapFrom(src => src.Product))
                .ForMember(dest => dest.UnitName, opt => opt.MapFrom(src => src.Unit != null ? src.Unit.Name : null))
                .ForMember(dest => dest.UnitCode, opt => opt.MapFrom(src => src.Unit != null ? src.Unit.Code : null));

            CreateMap<StockTransferRequest, StockTransferViewDto>()
                .ForMember(dest => dest.Items, opt => opt.MapFrom(src => src.StockTransferItems))
                .ForMember(dest => dest.FromWarehouseName, opt => opt.MapFrom(src => src.FromWarehouse.Name))
                .ForMember(dest => dest.ToWarehouseName, opt => opt.MapFrom(src => src.ToWarehouse.Name))
                .ForMember(dest => dest.RejectReason,
                    opt => opt.MapFrom(src => src.Status == "Rejected" ? "Check logs for details" : null));
        }
    }
}