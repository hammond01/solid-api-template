namespace SolidTemplate.API.Controllers;

[Route("api/[controller]")]
[ApiController]
public class AccountController : ControllerBase
{
    private readonly IAccountManager _accountManager;
    public AccountController(IAccountManager accountManager)
    {
        _accountManager = accountManager;
    }

    [HttpPost("login")]
    [AllowAnonymous]
    [ProducesResponseType(Status204NoContent)]
    [ProducesResponseType(Status401Unauthorized)]
    public async Task<ApiResponse> Login(LoginRequest parameters) => ModelState.IsValid
        ? await _accountManager.Login(parameters)
        : new ApiResponse(Status400BadRequest, "InvalidData");

    [HttpPost("register")]
    [AllowAnonymous]
    [ProducesResponseType(Status200OK)]
    [ProducesResponseType(Status400BadRequest)]
    public async Task<ApiResponse> Register(RegisterRequest parameters) => ModelState.IsValid
        ? await _accountManager.Register(parameters)
        : new ApiResponse(Status400BadRequest, "InvalidData");

    [HttpPost("confirm-email")]
    [AllowAnonymous]
    [ProducesResponseType(Status200OK)]
    [ProducesResponseType(Status400BadRequest)]
    public async Task<ApiResponse> ConfirmEmail(ConfirmEmailDto parameters) => ModelState.IsValid
        ? await _accountManager.ConfirmEmail(parameters)
        : new ApiResponse(Status400BadRequest, "InvalidData");

    [HttpPost("update-user")]
    [Authorize]
    [ProducesResponseType(Status200OK)]
    [ProducesResponseType(Status400BadRequest)]
    public async Task<ApiResponse> AdminUpdateUser(UserDto userDto) => ModelState.IsValid
        ? await _accountManager.AdminUpdateUser(userDto)
        : new ApiResponse(Status400BadRequest, "InvalidData");

    [HttpPost("reset-user-password")]
    [Authorize]
    [ProducesResponseType(Status200OK)]
    [ProducesResponseType(Status400BadRequest)]
    public async Task<ApiResponse> AdminResetUserPassword(ChangePasswordDto changePasswordDto) => ModelState.IsValid
        ? await _accountManager.AdminResetUserPasswordAsync(changePasswordDto, User)
        : new ApiResponse(Status400BadRequest, "InvalidData");
}
