using SolidTemplate.Application.Managers;
using SolidTemplate.Domain.DTOs.AdminDto;
namespace SolidTemplate.Persistence.Managers;

public class AdminManager : IAdminManager
{
    private readonly RoleManager<IdentityRole> _roleManager;
    public AdminManager(RoleManager<IdentityRole> roleManager)
    {
        _roleManager = roleManager;
    }

    public async Task<ApiResponse> GetRolesAsync(Int32 pageSize = 0, Int32 pageNumber = 0)
    {
        var roleQuery = _roleManager.Roles.AsQueryable().OrderBy(x => x.Name);
        var count = roleQuery.Count();
        var listResponse = (pageSize > 0 ? roleQuery.Skip(pageNumber * pageSize).Take(pageSize) : roleQuery).ToList();

        var roleDtoList = new List<RoleDto>();

        foreach (var role in listResponse)
        {
            var claims = await _roleManager.GetClaimsAsync(role);
            // List<string> permissions = claims.OrderBy(x => x.Value).Where(x => x.Type == ApplicationClaimTypes.Permission)
            //     .Select(x => _entityPermissions.GetPermissionByValue(x.Value).Name).ToList();

            roleDtoList.Add(new RoleDto
            {
                Name = role.Name ?? "", Permissions = []
            });
        }

        return new ApiResponse(Status200OK, $"{count} roles fetched", roleDtoList);
    }
    public Task<ApiResponse> GetRoleAsync(String name) => throw new NotImplementedException();
    public Task<ApiResponse> CreateRoleAsync(RoleDto roleDto) => throw new NotImplementedException();
    public Task<ApiResponse> UpdateRoleAsync(RoleDto roleDto) => throw new NotImplementedException();
    public Task<ApiResponse> DeleteRoleAsync(String name) => throw new NotImplementedException();
}
