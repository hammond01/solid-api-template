using System.IdentityModel.Tokens.Jwt;
using System.Text;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using SolidTemplate.Constants.ConfigurationOptions;
using JwtRegisteredClaimNames=Microsoft.IdentityModel.JsonWebTokens.JwtRegisteredClaimNames;
namespace SolidTemplate.Persistence.Managers;

public class AccountManager : IAccountManager
{
    private readonly IDatabaseInitializer _databaseInitializer;
    private readonly EntityPermissions _entityPermissions;
    private readonly IdentityConfig _identityConfig;
    private readonly ILogger<AccountManager> _logger;
    private readonly RoleManager<IdentityRole> _roleManager;
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly UserManager<ApplicationUser> _userManager;


    public AccountManager(SignInManager<ApplicationUser> signInManager, UserManager<ApplicationUser> userManager,
        IDatabaseInitializer databaseInitializer, ILogger<AccountManager> logger, IOptions<IdentityConfig> identityConfig,
        RoleManager<IdentityRole> roleManager, EntityPermissions entityPermissions)
    {
        _signInManager = signInManager;
        _userManager = userManager;
        _databaseInitializer = databaseInitializer;
        _logger = logger;
        _roleManager = roleManager;
        _entityPermissions = entityPermissions;
        _identityConfig = identityConfig.Value;
    }

    public async Task<ApiResponse> Login(LoginRequest parameters)
    {
        try
        {
            await _databaseInitializer.EnsureAdminIdentitiesAsync();

            var result = await _signInManager.PasswordSignInAsync(parameters.UserName, parameters.Password, parameters.RememberMe,
            true);

            if (result.RequiresTwoFactor)
            {
                _logger.LogInformation("Two factor authentication required for user " + parameters.UserName);

                return new ApiResponse(Status401Unauthorized, "Two factor authentication required")
                {
                    Result = new LoginResponseModel
                    {
                        RequiresTwoFactor = true
                    }
                };
            }

            if (result.IsLockedOut)
            {
                _logger.LogInformation("User Locked out: " + parameters.UserName);
                return new ApiResponse(Status401Unauthorized, "LockedUser");
            }

            if (result.IsNotAllowed)
            {
                _logger.LogInformation($"User {parameters.UserName} not allowed to log in, because email is not confirmed");
                return new ApiResponse(Status401Unauthorized, "EmailNotConfirmed");
            }

            if (result.Succeeded)
            {
                var user = await _userManager.FindByNameAsync(parameters.UserName);
                _logger.LogInformation($"Logged In user {parameters.UserName}");

                var authClaims = new List<Claim>
                {
                    new Claim(ClaimTypes.Name, user!.UserName!), new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
                };

                var userRoles = await _userManager.GetRolesAsync(user);
                authClaims.AddRange(userRoles.Select(userRole => new Claim(ClaimTypes.Role, userRole)));

                var roleQuery = _roleManager.Roles.AsQueryable().OrderBy(x => x.Name);
                var listResponse = roleQuery.ToList();
                var userPermissions = new List<string>();
                var roleDtoList = new List<RoleDto>();
                foreach (var role in listResponse)
                {
                    var claims = await _roleManager.GetClaimsAsync(role);
                    var permissions = claims.OrderBy(x => x.Value).Where(x => x.Type == ApplicationClaimTypes.Permission)
                        .Select(x => _entityPermissions.GetPermissionByValue(x.Value).Name).ToList();

                    roleDtoList.Add(new RoleDto
                    {
                        Name = role.Name!, Permissions = permissions
                    });
                }
                foreach (var role in userRoles)
                {
                    var roleDto = roleDtoList.FirstOrDefault(r => r.Name == role);
                    if (roleDto != null)
                    {
                        userPermissions.AddRange(roleDto.Permissions);
                    }
                }

                userPermissions = userPermissions.Distinct().ToList();

                authClaims.AddRange(userPermissions.Select(permission => new Claim("Permission", permission)));

                var secretKey = _identityConfig.SECRET;
                var authKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));

                var token = new JwtSecurityToken(
                _identityConfig.ISSUER,
                _identityConfig.AUDIENCE,
                expires: DateTime.Now.AddMinutes(5),
                claims: authClaims,
                signingCredentials: new SigningCredentials(authKey, SecurityAlgorithms.HmacSha256));

                var loginResponse = new LoginResponse
                {
                    Token = new JwtSecurityTokenHandler().WriteToken(token), RefreshToken = ""// Cấu hình refresh token nếu cần
                };
                return new ApiResponse(Status200OK, "LoginSuccess", loginResponse);
            }
            _logger.LogInformation($"Invalid Password for user {parameters.UserName}");
            return new ApiResponse(Status401Unauthorized, "LoginFailed");
        }
        catch (Exception ex)
        {
            _logger.LogError($"Login Failed: {ex.GetBaseException().Message}");
            return new ApiResponse(Status500InternalServerError, "LoginFailed");
        }
    }

    public async Task<ApiResponse> Register(RegisterRequest parameters)
    {
        if (string.IsNullOrWhiteSpace(parameters.UserName) ||
            string.IsNullOrWhiteSpace(parameters.Password))
        {
            return new ApiResponse(404, "Username and password are required.");
        }

        var existingUser = await _userManager.FindByNameAsync(parameters.UserName);
        if (existingUser != null)
        {
            return new ApiResponse(404, "User already exists.");
        }

        var newUser = new ApplicationUser
        {
            UserName = parameters.UserName,
            FirstName = parameters.FirstName,
            LastName = parameters.LastName,
            PhoneNumber = parameters.PhoneNumber
        };

        var result = await _userManager.CreateAsync(newUser, parameters.Password);
        if (result.Succeeded)
        {
            return new ApiResponse(200, "User registered successfully.");
        }
        var errors = string.Join(", ", result.Errors.Select(e => e.Description));
        return new ApiResponse(404, $"Registration failed: {errors}");
    }
}
