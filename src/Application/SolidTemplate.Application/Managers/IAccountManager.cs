using System.Security.Claims;
using SolidTemplate.Domain.Common;
using SolidTemplate.Share.DTOs.UserDto;
namespace SolidTemplate.Application.Managers;

public interface IAccountManager
{
    Task<ApiResponse> Login(LoginRequest parameters);
    Task<ApiResponse> RefreshToken(string token, string refreshToken);
    Task<ApiResponse> Logout(ClaimsPrincipal authenticatedUser);
    Task<ApiResponse> Register(RegisterRequest parameters);
    Task<ApiResponse> ConfirmEmail(ConfirmEmailDto parameters);
    Task<ApiResponse> AdminUpdateUser(UserDto userDto);
    Task<ApiResponse> AdminResetUserPasswordAsync(ChangePasswordDto changePasswordDto, ClaimsPrincipal authenticatedUser);
}
