namespace SolidTemplate.Infrastructure.Storage;

public interface IDatabaseInitializer
{
    Task SeedAsync();
    Task EnsureAdminIdentitiesAsync();
}
