using AutoMapper;
using warehouseManagement.DTOs;
using warehouseManagement.Models;

namespace warehouseManagement.Mappers
{
    public class PurchaseProfile : Profile
    {

        public PurchaseProfile()
        {
            CreateMap<InBoundRequestDTOs, InboundRequest>();
            CreateMap<InBoundIteamDtos, InboundItem>();
        }
    }
}
