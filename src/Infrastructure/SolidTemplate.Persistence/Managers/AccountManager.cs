namespace SolidTemplate.Persistence.Managers;

public class AccountManager : IAccountManager
{
    private readonly IConfiguration _configuration;
    private readonly IDatabaseInitializer _databaseInitializer;
    private readonly ILogger<AccountManager> _logger;
    private readonly RoleManager<IdentityRole> _roleManager;
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly UserManager<ApplicationUser> _userManager;

    public AccountManager(SignInManager<ApplicationUser> signInManager, UserManager<ApplicationUser> userManager,
        IDatabaseInitializer databaseInitializer, RoleManager<IdentityRole> roleManager, ILogger<AccountManager> logger,
        IConfiguration configuration)
    {
        _signInManager = signInManager;
        _userManager = userManager;
        _databaseInitializer = databaseInitializer;
        _roleManager = roleManager;
        _logger = logger;
        _configuration = configuration;
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

            // If lock out activated and the max. amounts of attempts is reached. 
            if (result.IsLockedOut)
            {
                _logger.LogInformation("User Locked out: " + parameters.UserName);
                return new ApiResponse(Status401Unauthorized, "LockedUser");
            }

            // If your email is not confirmed, but you require it in the settings for login.
            if (result.IsNotAllowed)
            {
                _logger.LogInformation($"User {parameters.UserName} not allowed to log in, because email is not confirmed");
                return new ApiResponse(Status401Unauthorized, "EmailNotConfirmed");
            }

            if (result.Succeeded)
            {
                var user = await _userManager.FindByNameAsync(parameters.UserName);
                _logger.LogInformation($"Logged In user {parameters.UserName}");

                //TODO parameters.IsValidReturnUrl is set true above 
                //if (!parameters.IsValidReturnUrl)
                //    // user might have clicked on a malicious link - should be logged
                //    throw new Exception("invalid return URL");

                return new ApiResponse(Status200OK, "LoginSuccess");
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
