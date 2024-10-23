namespace SolidTemplate.Persistence;

public class DatabaseInitializer : IDatabaseInitializer
{
    private readonly ApplicationDbContext _context;
    private readonly RoleManager<IdentityRole> _roleManager;
    private readonly UserManager<ApplicationUser> _userManager;

    public DatabaseInitializer(ApplicationDbContext context, UserManager<ApplicationUser> userManager,
        RoleManager<IdentityRole> roleManager)
    {
        _context = context;
        _userManager = userManager;
        _roleManager = roleManager;
    }
    public async Task SeedAsync()
    {
        await MigrateAsync();
        await EnsureAdminIdentitiesAsync();
        await _context.Database.ExecuteSqlRawAsync("SET TRANSACTION ISOLATION LEVEL READ UNCOMMITTED;");
    }
    public async Task EnsureAdminIdentitiesAsync()
    {
        // Đảm bảo vai trò Administrator đã tồn tại
        await EnsureRoleAsync(DefaultRoleNames.Administrator, null);

        // Tạo user Administrator
        await CreateUserAsync(DefaultUserNames.Administrator, "admin123@", "Admin", "Template", "admin@template.com",
        "+84 (123) 456-7890", new[]
        {
            DefaultRoleNames.Administrator
        });

        var adminRole = await _roleManager.FindByNameAsync(DefaultRoleNames.Administrator);

        var roleClaims = (await _roleManager.GetClaimsAsync(adminRole!)).Select(c => c.Value).ToList();

        string[] claimsToAdd =
        [
            ApplicationClaimTypes.Permission
        ];

        foreach (var claim in claimsToAdd.Except(roleClaims))
        {
            await _roleManager.AddClaimAsync(adminRole!, new Claim(claim, "true"));
        }

        var deprecatedClaims = roleClaims.Except(claimsToAdd);

        foreach (var claim in deprecatedClaims)
        {
            await _roleManager.RemoveClaimAsync(adminRole!, new Claim(claim, "true"));
        }
    }


    private async Task MigrateAsync() => await _context.Database.MigrateAsync();

    private async Task EnsureRoleAsync(string roleName, string[]? claims)
    {
        if (await _roleManager.FindByNameAsync(roleName) == null)
        {
            var role = new IdentityRole(roleName);
            var result = await _roleManager.CreateAsync(role);

            if (!result.Succeeded)
            {
                throw new Exception(result.Errors.First().Description);
            }
        }

        if (claims != null && claims.Length != 0)
        {
            var existingRole = await _roleManager.FindByNameAsync(roleName);
            var roleClaims = await _roleManager.GetClaimsAsync(existingRole!);

            foreach (var claim in claims.Except(roleClaims.Select(c => c.Type)))
            {
                var result = await _roleManager.AddClaimAsync(existingRole!, new Claim(claim, ClaimValues.TrueString));
                if (!result.Succeeded)
                {
                    throw new Exception(result.Errors.First().Description);
                }
            }
        }
    }


    private async Task CreateUserAsync(String userName, String password, String firstName, String lastName,
        String email, String phoneNumber, String[]? roles = null)
    {
        var applicationUser = _userManager.FindByNameAsync(userName).Result;

        if (applicationUser != null)
        {
            return;
        }
        applicationUser = new ApplicationUser
        {
            UserName = userName,
            Email = email,
            PhoneNumber = phoneNumber,
            FirstName = firstName,
            LastName = lastName,
            EmailConfirmed = true
        };

        var result = _userManager.CreateAsync(applicationUser, password).Result;

        if (!result.Succeeded)
        {
            throw new Exception(result.Errors.First().Description);
        }

        result = _userManager.AddClaimsAsync(applicationUser, new[]
        {
            new Claim(ClaimTypes.Name, userName),
            new Claim(ClaimTypes.GivenName, firstName),
            new Claim(ClaimTypes.Surname, lastName),
            new Claim(ClaimTypes.Email, email),
            new Claim(ApplicationClaimTypes.EmailVerified, ClaimValues.TrueString, ClaimValueTypes.Boolean),
            new Claim(ClaimTypes.HomePhone, phoneNumber)
        }).Result;

        if (!result.Succeeded)
        {
            throw new Exception(result.Errors.First().Description);
        }

        //add claims version of roles
        if (roles == null)
        {
            return;
        }
        foreach (var role in roles.Distinct())
        {
            await _userManager.AddClaimAsync(applicationUser, new Claim($"Is{role}", ClaimValues.TrueString));
        }

        var user = await _userManager.FindByNameAsync(applicationUser.UserName);

        try
        {
            if (user != null)
            {
                result = await _userManager.AddToRolesAsync(user, roles.Distinct());
            }
        }
        catch
        {
            if (user != null)
            {
                await _userManager.DeleteAsync(user);
            }
            throw;
        }

        if (result.Succeeded)
        {
            return;
        }
        if (user != null)
        {
            await _userManager.DeleteAsync(user);
        }

    }
}
