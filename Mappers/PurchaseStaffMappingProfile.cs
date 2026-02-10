using AutoMapper;
using warehouseManagement.Models;
using static warehouseManagement.DTOs.PurchaseStaffDTO;

namespace warehouseManagement.Mappers
{
    public class PurchaseStaffMappingProfile : Profile
    {
        public PurchaseStaffMappingProfile()
        {
            CreateMap<InboundRequestCreateDto, InboundRequest>();
            CreateMap<InboundItemDto, InboundItem>();

            CreateMap<InboundRequest, InboundRequestViewDto>()
                .ForMember(dest => dest.RejectReason, opt => opt.MapFrom(src =>
                    // Lấy comment cuối cùng nếu trạng thái là Rejected
                    src.Status == "Rejected" ? "Check logs for details" : ""));
        }
        }
}

