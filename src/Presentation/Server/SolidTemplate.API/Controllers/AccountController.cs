using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using SolidTemplate.Application.Managers;
using SolidTemplate.Domain.Common;
using SolidTemplate.Shared.DTOs.UserDto;
using SolidTemplate.Shared.Resources;
using static Microsoft.AspNetCore.Http.StatusCodes;

namespace SolidTemplate.API.Controllers;

[Route("api/[controller]")]
[ApiController]
public class AccountController : ControllerBase
{
    private readonly IAccountManager _accountManager;
    private readonly IStringLocalizer<AppStrings> _localizer;
    public AccountController(IAccountManager accountManager, IStringLocalizer<AppStrings> localizer)
    {
        _accountManager = accountManager;
        _localizer = localizer;
    }

    [HttpPost("login")]
    [AllowAnonymous]
    [ProducesResponseType(Status204NoContent)]
    [ProducesResponseType(Status401Unauthorized)]
    public async Task<ApiResponse> Login(LoginRequest parameters) => ModelState.IsValid
        ? await _accountManager.Login(parameters)
        : new ApiResponse(Status400BadRequest, _localizer[nameof(AppStrings.InvalidData)]);

    [HttpPost("refresh-token")]
    public async Task<IActionResult> RefreshToken([FromBody] RefreshTokenDto request)
    {
        var response = await _accountManager.RefreshToken(request.Token, request.RefreshToken);
        return Ok(response);
    }

    [HttpPost("register")]
    [AllowAnonymous]
    [ProducesResponseType(Status200OK)]
    [ProducesResponseType(Status400BadRequest)]
    public async Task<ApiResponse> Register(RegisterRequest parameters) => ModelState.IsValid
        ? await _accountManager.Register(parameters)
        : new ApiResponse(Status400BadRequest, _localizer[nameof(AppStrings.InvalidData)]);

    [HttpPost("confirm-email")]
    [AllowAnonymous]
    [ProducesResponseType(Status200OK)]
    [ProducesResponseType(Status400BadRequest)]
    public async Task<ApiResponse> ConfirmEmail(ConfirmEmailDto parameters) => ModelState.IsValid
        ? await _accountManager.ConfirmEmail(parameters)
        : new ApiResponse(Status400BadRequest, _localizer[nameof(AppStrings.InvalidData)]);

    [HttpPost("update-user")]
    [Authorize]
    [ProducesResponseType(Status200OK)]
    [ProducesResponseType(Status400BadRequest)]
    public async Task<ApiResponse> AdminUpdateUser(UserDto userDto) => ModelState.IsValid
        ? await _accountManager.AdminUpdateUser(userDto)
        : new ApiResponse(Status400BadRequest, _localizer[nameof(AppStrings.InvalidData)]);

    [HttpPost("reset-user-password")]
    [Authorize]
    [ProducesResponseType(Status200OK)]
    [ProducesResponseType(Status400BadRequest)]
    public async Task<ApiResponse> AdminResetUserPassword(ChangePasswordDto changePasswordDto) => ModelState.IsValid
        ? await _accountManager.AdminResetUserPasswordAsync(changePasswordDto, User)
        : new ApiResponse(Status400BadRequest, _localizer[nameof(AppStrings.InvalidData)]);
}
