namespace SolidTemplate.API.Authentication;

public static class AddAuthorizationPolicies
{
    public static void RegisterPolicy(this IServiceCollection services) => services.AddAuthorizationBuilder()
        .AddPolicy("AdminPolicy", configurePolicy: policy =>
            policy.RequireRole("Admin"))
        .AddPolicy("ProductPolicy", configurePolicy: policy =>
            policy.RequireRole("ProductManager", "Admin"))
        .AddPolicy("ProductStaffPolicy", configurePolicy: policy =>
            policy.RequireRole("ProductStaff", "ProductManager", "Admin"));
}
