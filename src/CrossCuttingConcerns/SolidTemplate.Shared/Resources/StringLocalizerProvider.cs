using System.Reflection;
using Microsoft.Extensions.Localization;
using SolidTemplate.Shared.Attributes;
namespace SolidTemplate.Shared.Resources;

public static class StringLocalizerProvider
{
    public static IStringLocalizer ProvideLocalizer(Type dtoType, IStringLocalizerFactory factory)
        => factory.Create(dtoType.GetCustomAttribute<DtoResourceTypeAttribute>()?.ResourceType ?? typeof(AppStrings));
}
