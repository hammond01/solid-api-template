using AutoMapper;
using SolidTemplate.Domain.DataModels;
using SolidTemplate.Domain.DTOs.CategoryDto;
namespace SolidTemplate.Application.Decorators;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        CreateMap<GetCategoryDto, Category>().ReverseMap();
        CreateMap<CreateCategoryDto, Category>().ReverseMap();
    }
}
