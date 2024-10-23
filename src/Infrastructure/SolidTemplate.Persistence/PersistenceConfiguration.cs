﻿using SolidTemplate.CrossCuttingConcerns.Permission;
namespace SolidTemplate.Persistence;

public static class PersistenceConfiguration
{
    public static IServiceCollection AddPersistence(this IServiceCollection services, string connectionString,
        string migrationsAssembly = "")
    {
        services.AddDbContext<ApplicationDbContext>(options => options.UseSqlServer(connectionString, sqlServerOptionsAction: sql =>
            {
                if (!string.IsNullOrEmpty(migrationsAssembly))
                {
                    sql.MigrationsAssembly(migrationsAssembly);
                }
            }))
            .AddDbContextFactory<ApplicationDbContext>((Action<DbContextOptionsBuilder>)null!, ServiceLifetime.Scoped);

        services.AddTransient<IDatabaseInitializer, DatabaseInitializer>();
        services.AddScoped<IAccountManager, AccountManager>();
        services.AddScoped<IAdminManager, AdminManager>();

        services.AddScoped<EntityPermissions>();

        return services;
    }
}
