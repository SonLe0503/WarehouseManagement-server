using AutoMapper;
using warehouseManagement.DTOs;
using warehouseManagement.Models;

namespace warehouseManagement.Mappers
{
    public class ProductProfile : Profile
    {
        public ProductProfile()
        {
            CreateMap<Product, ProductDTO>()
                .ForMember(dest => dest.CategoryName,
                opt => opt.MapFrom(src => src.Category.Name))
                .ForMember(dest => dest.BaseUnitId,
                opt => opt.MapFrom(src => src.BaseUnitId))
                .ForMember(dest => dest.BaseUnitCode,
                opt => opt.MapFrom(src => src.BaseUnit.Code));

            CreateMap<CreateProductDTO, Product>()
                .ForMember(dest => dest.Status, opt => opt.MapFrom(_ => "ACTIVE"))
                .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(_ => DateTime.Now));

            CreateMap<UpdateProductDTO, Product>()
                .ForAllMembers(opt => opt.Condition(
                    (src, dest, srcMember) => srcMember != null
                ));
        }
    }
}