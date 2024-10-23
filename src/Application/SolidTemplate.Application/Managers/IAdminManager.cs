namespace SolidTemplate.Application.Managers;

public interface IAdminManager
{
    Task<ApiResponse> GetRolesAsync(int pageSize = 0, int pageNumber = 0);

    Task<ApiResponse> GetRoleAsync(string name);

    Task<ApiResponse> CreateRoleAsync(RoleDto roleDto);

    Task<ApiResponse> UpdateRoleAsync(RoleDto roleDto);

    Task<ApiResponse> DeleteRoleAsync(string name);
}
