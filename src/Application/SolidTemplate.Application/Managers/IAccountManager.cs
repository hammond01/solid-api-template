using SolidTemplate.Domain.DTOs.UserDto;
namespace SolidTemplate.Application.Managers;

public interface IAccountManager
{
    Task<ApiResponse> Login(LoginRequest parameters);
    Task<ApiResponse> Register(RegisterRequest parameters);
}
