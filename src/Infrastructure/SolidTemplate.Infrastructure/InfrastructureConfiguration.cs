namespace SolidTemplate.Infrastructure;

public static class InfrastructureConfiguration
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services)
    {
        services.AddSingleton<IAuthorizationPolicyProvider, AuthorizationPolicyProvider>();
        services.AddTransient<IAuthorizationHandler, PermissionRequirementHandler>();
        services.AddTransient<IAuthorizationHandler, DomainRequirementHandler>();
        services.AddTransient<IAuthorizationHandler, EmailVerifiedHandler>();
        return services;
    }
}
