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
    [Authorize]
    [HttpGet]
    public IActionResult Get() => Ok("Hello World");

    [HttpPost("Login")]
    [AllowAnonymous]
    [ProducesResponseType(Status204NoContent)]
    [ProducesResponseType(Status401Unauthorized)]
    public async Task<ApiResponse> Login(LoginRequest parameters) => ModelState.IsValid
        ? await _accountManager.Login(parameters)
        : new ApiResponse(Status400BadRequest, "InvalidData");

    [HttpPost("Register")]
    [AllowAnonymous]
    [ProducesResponseType(Status200OK)]
    [ProducesResponseType(Status400BadRequest)]
    public async Task<ApiResponse> Register(RegisterRequest parameters) => ModelState.IsValid
        ? await _accountManager.Register(parameters)
        : new ApiResponse(Status400BadRequest, "InvalidData");
}
