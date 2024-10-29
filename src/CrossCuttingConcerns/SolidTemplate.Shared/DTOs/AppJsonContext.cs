using System.Text.Json.Serialization;
using SolidTemplate.Shared.DTOs.AdminDto;
using SolidTemplate.Shared.DTOs.CategoryDto;
using SolidTemplate.Shared.DTOs.EmailDto;
using SolidTemplate.Shared.DTOs.UserDto;
namespace SolidTemplate.Shared.DTOs;

/// <summary>
///     https://devblogs.microsoft.com/dotnet/try-the-new-system-text-json-source-generator/
/// </summary>
[JsonSourceGenerationOptions(PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase)]
[JsonSerializable(typeof(Dictionary<string, object>))]
[JsonSerializable(typeof(RoleDto))]
[JsonSerializable(typeof(CreateCategoryDto))]
[JsonSerializable(typeof(GetCategoryDto))]
[JsonSerializable(typeof(EmailAddressDto))]
[JsonSerializable(typeof(EmailMessageDto))]
[JsonSerializable(typeof(AccountFormModel))]
[JsonSerializable(typeof(ChangePasswordDto))]
[JsonSerializable(typeof(ConfirmEmailDto))]
[JsonSerializable(typeof(LoginRequest))]
[JsonSerializable(typeof(LoginResponse))]
[JsonSerializable(typeof(LoginResponseModel))]
[JsonSerializable(typeof(RefreshTokenDto))]
[JsonSerializable(typeof(RegisterRequest))]
[JsonSerializable(typeof(UpdateUserDto))]
[JsonSerializable(typeof(UserDto.UserDto))]
public partial class AppJsonContext : JsonSerializerContext
{
}
