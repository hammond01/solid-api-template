using SolidTemplate.Infrastructure.DateTimes;
using TSTemplate.CrossCuttingConcerns.DateTimes;
// ReSharper disable once CheckNamespace
namespace Microsoft.Extensions.DependencyInjection;

public static class DateTimeProviderExtensions
{
    public static IServiceCollection AddDateTimeProvider(this IServiceCollection services)
    {
        _ = services.AddSingleton<IDateTimeProvider, DateTimeProvider>();
        return services;
    }
}
