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
                .ForMember(dest => dest.Roles, opt => opt.MapFrom(src => src.Roles.Select(r => r.Name).ToList()));
            CreateMap<DTOs.UpdateUserDTO, Models.User>()
                .ForAllMembers(opt => opt.Condition((src, dest, srcMember) => srcMember != null));
        }
    }
}
