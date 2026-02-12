using AutoMapper;
using warehouseManagement.DTOs;
using warehouseManagement.Models;

namespace warehouseManagement.Mappings
{
    public class WarehouseProfile : Profile
    {
        public WarehouseProfile()
        {
            CreateMap<Warehouse, WarehouseLookupDto>();
        }
    }
}