using Microsoft.AspNetCore.Http;
namespace SolidTemplate.Persistence.Managers;

public class AdminManager : IAdminManager
{
    private readonly EntityPermissions _entityPermissions;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly RoleManager<IdentityRole> _roleManager;
    private readonly UserManager<ApplicationUser> _userManager;

    public AdminManager(RoleManager<IdentityRole> roleManager, EntityPermissions entityPermissions,
        UserManager<ApplicationUser> userManager, IHttpContextAccessor httpContextAccessor)
    {
        _roleManager = roleManager;
        _entityPermissions = entityPermissions;
        _userManager = userManager;
        _httpContextAccessor = httpContextAccessor;
    }

    public async Task<ApiResponse> GetUsers(int pageSize = 10, int pageNumber = 0)
    {
        var userList = _userManager.Users.AsQueryable();
        var count = userList.Count();
        var listResponse = userList.OrderBy(x => x.Id).Skip(pageNumber * pageSize).Take(pageSize).ToList();
        var userDtoList =
            new List<UserDto>();
        foreach (var applicationUser in listResponse)
        {
            userDtoList.Add(new UserDto
            {
                FirstName = applicationUser.FirstName,
                LastName = applicationUser.LastName,
                UserName = applicationUser.UserName,
                Email = applicationUser.Email,
                UserId = new Guid(applicationUser.Id),
                Roles = await _userManager.GetRolesAsync(applicationUser).ConfigureAwait(true) as List<string>
            });
        }

        return new ApiResponse(Status200OK, $"{count} users fetched", userDtoList);
    }
    public ApiResponse GetPermissions()
    {
        var permissions = _entityPermissions.GetAllPermissionNames();
        return new ApiResponse(Status200OK, "Permissions list fetched", permissions);
    }
    public async Task<ApiResponse> GetRolesAsync(int pageSize = 0, int pageNumber = 0)
    {
        var roleQuery = _roleManager.Roles.AsQueryable().OrderBy(x => x.Name);
        var count = roleQuery.Count();
        var listResponse = (pageSize > 0 ? roleQuery.Skip(pageNumber * pageSize).Take(pageSize) : roleQuery).ToList();

        var roleDtoList = new List<RoleDto>();

        foreach (var role in listResponse)
        {
            var claims = await _roleManager.GetClaimsAsync(role);
            var permissions = claims.OrderBy(x => x.Value).Where(x => x.Type == ApplicationClaimTypes.Permission)
                .Select(x => _entityPermissions.GetPermissionByValue(x.Value).Name).ToList();

            roleDtoList.Add(new RoleDto
            {
                Name = role.Name ?? "", Permissions = permissions
            });
        }

        return new ApiResponse(Status200OK, $"{count} roles fetched", roleDtoList);
    }
    public async Task<ApiResponse> GetRoleAsync(string roleName)
    {
        var identityRole = await _roleManager.FindByNameAsync(roleName);

        var claims = await _roleManager.GetClaimsAsync(identityRole!);
        var permissions = claims.OrderBy(x => x.Value).Where(x => x.Type == ApplicationClaimTypes.Permission)
            .Select(x => _entityPermissions.GetPermissionByValue(x.Value).Name).ToList();

        var roleDto = new RoleDto
        {
            Name = roleName, Permissions = permissions
        };

        return new ApiResponse(Status200OK, "Role fetched", roleDto);
    }
    public async Task<ApiResponse> CreateRoleAsync(RoleDto roleDto)
    {
        if (_roleManager.Roles.Any(r => r.Name == roleDto.Name))
            return new ApiResponse(Status400BadRequest, $"Role {roleDto.Name} already exists");

        var result = await _roleManager.CreateAsync(new IdentityRole(roleDto.Name));

        if (!result.Succeeded)
        {
            var msg = result.GetErrors();
            return new ApiResponse(Status400BadRequest, msg);
        }

        var role = await _roleManager.FindByNameAsync(roleDto.Name);

        foreach (var claim in roleDto.Permissions)
        {
            var resultAddClaim = await _roleManager.AddClaimAsync(role!,
            new Claim(ApplicationClaimTypes.Permission, _entityPermissions.GetPermissionByName(claim)));

            if (!resultAddClaim.Succeeded)
                await _roleManager.DeleteAsync(role!);
        }

        return new ApiResponse(Status200OK, $"Role {roleDto.Name} created");
    }
    public async Task<ApiResponse> UpdateRoleAsync(RoleDto roleDto)
    {
        var response = new ApiResponse(Status200OK, $"Role {roleDto.Name} updated", roleDto);

        if (!_roleManager.Roles.Any(r => r.Name == roleDto.Name))
            response = new ApiResponse(Status400BadRequest, $"The role {roleDto.Name} doesn't exist");
        else
        {
            if (roleDto.Name == DefaultRoleNames.Administrator)
                response = new ApiResponse(Status403Forbidden, $"Role {roleDto.Name} cannot be edited");
            else
            {
                // Create the permissions
                var role = await _roleManager.FindByNameAsync(roleDto.Name);
                if (role == null)
                    response = new ApiResponse(Status400BadRequest, $"Role {roleDto.Name} not found");

                var claims = await _roleManager.GetClaimsAsync(role!);
                var permissions = claims.OrderBy(x => x.Value).Where(x => x.Type == ApplicationClaimTypes.Permission)
                    .Select(x => x.Value).ToList();

                foreach (var permission in permissions)
                    await _roleManager.RemoveClaimAsync(role!, new Claim(ApplicationClaimTypes.Permission, permission));

                foreach (var claim in roleDto.Permissions)
                {
                    var result = await _roleManager.AddClaimAsync(role!,
                    new Claim(ApplicationClaimTypes.Permission, _entityPermissions.GetPermissionByName(claim)));

                    if (!result.Succeeded)
                        await _roleManager.DeleteAsync(role!);
                }
            }
        }

        return response;
    }
    public async Task<ApiResponse> DeleteRoleAsync(string name)
    {
        var response = new ApiResponse(Status200OK, $"Role {name} deleted");

        // Check if the role is used by a user
        var users = await _userManager.GetUsersInRoleAsync(name);
        if (users.Any())
            response = new ApiResponse(Status404NotFound, "RoleInUseWarning", name);
        else
        {
            if (name == DefaultRoleNames.Administrator)
                response = new ApiResponse(Status403Forbidden, $"Role {name} cannot be deleted");
            else
            {
                // Delete the role
                var role = await _roleManager.FindByNameAsync(name);
                await _roleManager.DeleteAsync(role!);
            }
        }

        return response;
    }
    public async Task<ApiResponse> UpdateUserRoles(UpdateUserDto updateUserDto)
    {
        var user = await _userManager.FindByIdAsync(updateUserDto.UserId.ToString());
        if (user == null)
            return new ApiResponse(Status404NotFound, "User not found");

        var roles = await _userManager.GetRolesAsync(user);
        var result = await _userManager.RemoveFromRolesAsync(user, roles);
        if (!result.Succeeded)
            return new ApiResponse(Status400BadRequest, result.GetErrors());

        result = await _userManager.AddToRolesAsync(user, updateUserDto.Roles!);
        return !result.Succeeded
            ? new ApiResponse(Status400BadRequest, result.GetErrors())
            : new ApiResponse(Status200OK, "User roles updated");

    }
    public string GetUserLogin()
    {
        if (_httpContextAccessor.HttpContext is null)
            return "";
        var userName = _httpContextAccessor.HttpContext!.User.Identity!.Name!;
        return string.IsNullOrEmpty(userName) ? "" : userName.ToUpper();
    }
}
