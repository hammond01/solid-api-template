using Microsoft.AspNetCore.Mvc;
using SolidTemplate.Application.Managers;
using SolidTemplate.Domain.Common;
namespace SolidTemplate.API.Controllers;

[Route("api/[controller]")]
[ApiController]
public class AdminController : ControllerBase
{
    private readonly IAdminManager _adminManager;
    public AdminController(IAdminManager adminManager)
    {
        _adminManager = adminManager;
    }

    [HttpGet]
    public async Task<ApiResponse> GetRolesAsync(int pageSize = 0, int pageNumber = 0)
        => await _adminManager.GetRolesAsync(pageSize, pageNumber);
}
