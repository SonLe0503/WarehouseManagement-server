using AutoMapper;
using warehouseManagement.DTOs.Sessions;
using warehouseManagement.Models;

namespace warehouseManagement.Mappers
{
    public class StockCountProfile : Profile
    {
        public StockCountProfile()
        {
            CreateMap<StockCountSession, StockCountSessionDTO>();
            CreateMap<StockCountItem, StockCountItemDTO>();

        }
    }
}
