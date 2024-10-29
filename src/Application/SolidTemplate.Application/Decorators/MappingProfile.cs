using AutoMapper;
using SolidTemplate.Domain.DataModels;
using SolidTemplate.Shared.DTOs.CategoryDto;
using SolidTemplate.Shared.DTOs.UserDto;
namespace SolidTemplate.Application.Decorators;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        CreateMap<GetCategoryDto, Category>().ReverseMap();
        CreateMap<CreateCategoryDto, Category>().ReverseMap();
        CreateMap<UserDto, ApplicationUser>().ReverseMap();
    }
}
