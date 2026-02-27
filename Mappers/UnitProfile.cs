using AutoMapper;
using warehouseManagement.DTOs;
using warehouseManagement.Models;

namespace warehouseManagement.Mappers
{
    public class UnitProfile : Profile
    {
        public UnitProfile() 
        {
            CreateMap<Unit, UnitDTO>();
        }
    }
}
