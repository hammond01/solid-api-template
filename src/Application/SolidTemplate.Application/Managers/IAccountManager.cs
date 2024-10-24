using System.Security.Claims;
namespace SolidTemplate.Application.Managers;

public interface IAccountManager
{
    Task<ApiResponse> Login(LoginRequest parameters);
    Task<ApiResponse> Logout(ClaimsPrincipal authenticatedUser);
    Task<ApiResponse> Register(RegisterRequest parameters);
    Task<ApiResponse> ConfirmEmail(ConfirmEmailDto parameters);

    Task<ApiResponse> AdminUpdateUser(UserDto userDto);

    Task<ApiResponse> AdminResetUserPasswordAsync(ChangePasswordDto changePasswordDto, ClaimsPrincipal authenticatedUser);
}
