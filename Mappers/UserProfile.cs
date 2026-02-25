using AutoMapper;
using warehouseManagement.DTOs;
using warehouseManagement.Models;

namespace warehouseManagement.Mappers
{
    public class UserProfile : Profile
    {
        public UserProfile()
        {
            CreateMap<Models.User, DTOs.UserDTO>()
                .ForMember(dest => dest.Roles, opt => opt.MapFrom(src => src.Roles.Select(r => r.Name).ToList()))
                    .ForMember(dest => dest.WarehouseName,
        opt => opt.MapFrom(src => src.Warehouse != null ? src.Warehouse.Name : null));
            CreateMap<DTOs.UpdateUserDTO, Models.User>()
                .ForAllMembers(opt => opt.Condition((src, dest, srcMember) => srcMember != null));
        }
    }
}
