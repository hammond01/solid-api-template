using SolidTemplate.Application.DTOs.AdminDto;
using SolidTemplate.Application.DTOs.UserDto;
namespace SolidTemplate.API.Controllers;

[Route("api/[controller]/[action]")]
[ApiController]
public class AdminController : ControllerBase
{
    private readonly IAdminManager _adminManager;
    public AdminController(IAdminManager adminManager)
    {
        _adminManager = adminManager;
    }

    [HttpGet("get-users")]
    public async Task<ApiResponse> GetUsers(int pageSize = 10, int pageNumber = 0)
        => await _adminManager.GetUsers(pageSize, pageNumber);

    [HttpGet("get-permissions")]
    public ApiResponse GetPermissions()
        => _adminManager.GetPermissions();

    [HttpGet("get-roles")]
    [Authorize(Permissions.Role.Read)]
    public async Task<ApiResponse> GetRoles(int pageSize = 0, int pageNumber = 0)
        => await _adminManager.GetRolesAsync(pageSize, pageNumber);

    [HttpGet("get-role/{name}")]
    public async Task<ApiResponse> GetRole(string name)
        => await _adminManager.GetRoleAsync(name);

    [HttpPost("create-role")]
    public async Task<ApiResponse> CreateRole([FromBody] RoleDto roleDto)
        => await _adminManager.CreateRoleAsync(roleDto);

    [HttpPut("update-role")]
    public async Task<ApiResponse> UpdateRole([FromBody] RoleDto roleDto)
        => await _adminManager.UpdateRoleAsync(roleDto);

    [HttpDelete("delete-role/{name}")]
    public async Task<ApiResponse> DeleteRole(string name)
        => await _adminManager.DeleteRoleAsync(name);

    [HttpPut("update-user-role")]
    public async Task<ApiResponse> UpdateUserRoles([FromBody] UpdateUserDto updateUserDto)
        => await _adminManager.UpdateUserRoles(updateUserDto);
}
