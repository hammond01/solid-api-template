namespace SolidTemplate.Persistence.Managers;

public class AccountManager : IAccountManager
{
    private readonly ApplicationDbContext _context;
    private readonly IDatabaseInitializer _databaseInitializer;
    private readonly IDateTimeProvider _dateTimeProvider;
    private readonly EntityPermissions _entityPermissions;
    private readonly IdentityConfig _identityConfig;
    private readonly ILogger<AccountManager> _logger;
    private readonly RoleManager<IdentityRole> _roleManager;
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly UserManager<ApplicationUser> _userManager;

    public AccountManager(SignInManager<ApplicationUser> signInManager, UserManager<ApplicationUser> userManager,
        IDatabaseInitializer databaseInitializer, ILogger<AccountManager> logger, IOptions<IdentityConfig> identityConfig,
        RoleManager<IdentityRole> roleManager, EntityPermissions entityPermissions, ApplicationDbContext context,
        IDateTimeProvider dateTimeProvider)
    {
        _signInManager = signInManager;
        _userManager = userManager;
        _databaseInitializer = databaseInitializer;
        _logger = logger;
        _roleManager = roleManager;
        _entityPermissions = entityPermissions;
        _context = context;
        _dateTimeProvider = dateTimeProvider;
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
                authClaims.AddRange(userRoles.Select(userRole => new Claim(ApplicationClaimTypes.Role, userRole)));

                var roleQuery = _roleManager.Roles.AsQueryable().OrderBy(x => x.Name);
                var listResponse = roleQuery.ToList();
                var userPermissions = new List<string>();
                var roleDtoList = new List<RoleDto>();
                foreach (var role in listResponse)
                {
                    var claims = await _roleManager.GetClaimsAsync(role);
                    var permissions = claims.OrderBy(x => x.Value)
                        .Where(x => x.Type == ApplicationClaimTypes.Permission)
                        .Select(x => _entityPermissions.GetPermissionByValue(x.Value).Value).ToList();

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

                authClaims.AddRange(userPermissions.Select(permission => new Claim(ApplicationClaimTypes.Permission, permission)));

                var token = GenerateJwtToken(authClaims);
                var refreshToken = GenerateRefreshToken();
                await SaveRefreshTokenAsync(user.Id, refreshToken);
                var loginResponse = new LoginResponse
                {
                    Token = new JwtSecurityTokenHandler().WriteToken(token), RefreshToken = refreshToken.Token
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
    public async Task<ApiResponse> RefreshToken(string token, string refreshToken)
    {
        var principal = GetPrincipalFromExpiredToken(token);
        var userName = principal.Identity!.Name;

        var user = await _userManager.FindByNameAsync(userName!);
        if (user == null || !await ValidateRefreshTokenAsync(user.Id, refreshToken))
            return new ApiResponse(Status401Unauthorized, "Invalid refresh token");

        var authClaims = new List<Claim>
        {
            new Claim(ClaimTypes.Name, user.UserName!), new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

        var userRoles = await _userManager.GetRolesAsync(user);
        authClaims.AddRange(userRoles.Select(userRole => new Claim(ClaimTypes.Role, userRole)));
        authClaims.AddRange(userRoles.Select(userRole => new Claim(ApplicationClaimTypes.Role, userRole)));

        var userClaims = await _userManager.GetClaimsAsync(user);
        authClaims.AddRange(userClaims);

        var roleQuery = _roleManager.Roles.AsQueryable().OrderBy(x => x.Name);
        var listResponse = roleQuery.ToList();
        var userPermissions = new List<string>();
        var roleDtoList = new List<RoleDto>();

        foreach (var role in listResponse)
        {
            var claims = await _roleManager.GetClaimsAsync(role);
            var permissions = claims.OrderBy(x => x.Value)
                .Where(x => x.Type == ApplicationClaimTypes.Permission)
                .Select(x => _entityPermissions.GetPermissionByValue(x.Value).Value).ToList();

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

        authClaims.AddRange(userPermissions.Select(permission => new Claim(ApplicationClaimTypes.Permission, permission)));

        var newJwtToken = GenerateJwtToken(authClaims);

        var newRefreshToken = GenerateRefreshToken();
        await SaveRefreshTokenAsync(user.Id, newRefreshToken);

        var response = new LoginResponse
        {
            Token = new JwtSecurityTokenHandler().WriteToken(newJwtToken), RefreshToken = newRefreshToken.Token
        };

        return new ApiResponse(Status200OK, "TokenRefreshed", response);

    }
    public async Task<ApiResponse> Logout(ClaimsPrincipal authenticatedUser)
    {
        if (authenticatedUser.Identity!.IsAuthenticated)
        {
            await _signInManager.SignOutAsync();
        }
        return new ApiResponse(Status200OK, "Logout Successful");
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
    public async Task<ApiResponse> ConfirmEmail(ConfirmEmailDto parameters)
    {
        var user = await _userManager.FindByIdAsync(parameters.UserId);

        if (user == null)
        {
            _logger.LogInformation($"The user {parameters.UserId} doesn't exist");
            return new ApiResponse(Status404NotFound, "The user doesn't exist");
        }

        if (user.EmailConfirmed)
            return new ApiResponse(Status200OK, "EmailVerificationSuccessful");
        var token = parameters.Token;
        var result = await _userManager.ConfirmEmailAsync(user, token);

        if (!result.Succeeded)
        {
            var msg = result.GetErrors();
            return new ApiResponse(Status400BadRequest, msg);
        }

        await _userManager.RemoveClaimAsync(user,
        new Claim(ApplicationClaimTypes.EmailVerified, ClaimValues.FalseString, ClaimValueTypes.Boolean));
        await _userManager.AddClaimAsync(user,
        new Claim(ApplicationClaimTypes.EmailVerified, ClaimValues.TrueString, ClaimValueTypes.Boolean));

        return new ApiResponse(Status200OK, "EmailVerificationSuccessful");
    }
    public async Task<ApiResponse> AdminUpdateUser(UserDto userDto)
    {
        var user = await _userManager.FindByIdAsync(userDto.UserId.ToString());

        if (!user!.UserName!.Equals(DefaultUserNames.Administrator, StringComparison.CurrentCultureIgnoreCase) &&
            !userDto.UserName!.Equals(DefaultUserNames.Administrator, StringComparison.CurrentCultureIgnoreCase))
            user.UserName = userDto.UserName;

        user.FirstName = userDto.FirstName;
        user.LastName = userDto.LastName;
        user.Email = userDto.Email;

        try
        {
            await _userManager.UpdateAsync(user);
        }
        catch (Exception ex)
        {
            _logger.LogError($"Updating user exception: {ex.GetBaseException().Message}");
            return new ApiResponse(Status500InternalServerError, "Operation Failed");
        }

        if (userDto.Roles == null)
            return new ApiResponse(Status200OK, "Operation Successful");
        {
            try
            {
                var currentUserRoles = (List<string>)await _userManager.GetRolesAsync(user);

                var rolesToAdd = userDto.Roles.Where(newUserRole => !currentUserRoles.Contains(newUserRole)).ToList();

                if (rolesToAdd.Count > 0)
                {
                    await _userManager.AddToRolesAsync(user, rolesToAdd);

                    foreach (var role in rolesToAdd)
                        await _userManager.AddClaimAsync(user, new Claim($"Is{role}", ClaimValues.TrueString));
                }

                var rolesToRemove = currentUserRoles.Where(role => !userDto.Roles.Contains(role)).ToList();

                if (rolesToRemove.Count > 0)
                {
                    if (user.UserName.Equals(DefaultUserNames.Administrator, StringComparison.CurrentCultureIgnoreCase))
                        rolesToRemove.Remove(DefaultUserNames.Administrator);

                    await _userManager.RemoveFromRolesAsync(user, rolesToRemove);

                    foreach (var role in rolesToRemove)
                        await _userManager.RemoveClaimAsync(user, new Claim($"Is{role}", ClaimValues.TrueString));
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Updating Roles exception: {ex.GetBaseException().Message}");
                return new ApiResponse(Status500InternalServerError, "Operation Failed");
            }
        }

        return new ApiResponse(Status200OK, "Operation Successful");
    }
    public async Task<ApiResponse> AdminResetUserPasswordAsync(ChangePasswordDto changePasswordDto, ClaimsPrincipal authenticatedUser)
    {
        var user = await _userManager.FindByIdAsync(changePasswordDto.UserId);
        if (user == null)
        {
            _logger.LogWarning($"The user {changePasswordDto.UserId} doesn't exist");
            return new ApiResponse(Status404NotFound, "The user doesn't exist");
        }
        var passToken = await _userManager.GeneratePasswordResetTokenAsync(user);
        var result = await _userManager.ResetPasswordAsync(user, passToken, changePasswordDto.Password);
        if (result.Succeeded)
        {
            _logger.LogInformation(user.UserName + "'s password reset; Requested from Admin interface by:" +
                                   authenticatedUser.Identity!.Name);
            return new ApiResponse(Status204NoContent, user.UserName + " password reset");
        }
        _logger.LogWarning(user.UserName + "'s password reset failed; Requested from Admin interface by:" +
                           authenticatedUser.Identity!.Name);

        var msg = result.GetErrors();
        _logger.LogWarning($"Error while resetting password of {user.UserName}: {msg}");
        return new ApiResponse(Status400BadRequest, msg);
    }
    private RefreshToken GenerateRefreshToken() => new RefreshToken
    {
        Token = Convert.ToBase64String(RandomNumberGenerator.GetBytes(64)),
        Expires = _dateTimeProvider.UtcNow.AddDays(7),
        Created = _dateTimeProvider.UtcNow
    };

    private async Task SaveRefreshTokenAsync(string userId, RefreshToken refreshToken)
    {
        refreshToken.UserId = userId;
        await _context.RefreshTokens.AddAsync(refreshToken);
        await _context.SaveChangesAsync();
    }
    private ClaimsPrincipal GetPrincipalFromExpiredToken(string token)
    {
        var tokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = _identityConfig.ISSUER,
            ValidAudience = _identityConfig.AUDIENCE,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_identityConfig.SECRET))
        };

        var tokenHandler = new JwtSecurityTokenHandler();
        var principal = tokenHandler.ValidateToken(token, tokenValidationParameters, out var securityToken);

        if (securityToken is not JwtSecurityToken jwtSecurityToken ||
            !jwtSecurityToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha256, StringComparison.InvariantCultureIgnoreCase))
            throw new SecurityTokenException("Invalid token");

        return principal;
    }

    private async Task<bool> ValidateRefreshTokenAsync(string userId, string refreshToken)
    {
        var storedToken = await _context.RefreshTokens
            .Where(t => t.UserId == userId && t.Token == refreshToken)
            .FirstOrDefaultAsync();
        return storedToken is { IsExpired: false, Revoked: null };
    }

    private JwtSecurityToken GenerateJwtToken(List<Claim> claims)
    {
        var secretKey = _identityConfig.SECRET;
        var authKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));

        return new JwtSecurityToken(
        _identityConfig.ISSUER,
        _identityConfig.AUDIENCE,
        expires: _dateTimeProvider.UtcNow.AddMinutes(5),
        claims: claims,
        signingCredentials: new SigningCredentials(authKey, SecurityAlgorithms.HmacSha256));
    }
}
