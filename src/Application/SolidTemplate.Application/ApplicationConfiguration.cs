using Microsoft.Extensions.DependencyInjection;
using SolidTemplate.Application.Decorators;
namespace SolidTemplate.Application;

public static class ApplicationConfiguration
{
    public static IServiceCollection ApplicationConfigureServices(this IServiceCollection services)
    {
        services.AddAutoMapper(typeof(MappingProfile));
        return services;
    }
}
