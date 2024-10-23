using AutoMapper;
using Microsoft.EntityFrameworkCore;
using SolidTemplate.Domain.DTOs.CategoryDto;
namespace SolidTemplate.API.Controllers;

[Route("api/[controller]")]
[ApiController]
public class CategoryController : ControllerBase
{
    private readonly ApplicationDbContext _context;
    private readonly IMapper _mapper;
    public CategoryController(ApplicationDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }
    // [Authorize]
    [HttpGet]
    public async Task<ActionResult<IEnumerable<GetCategoryDto>>> GetCategories()
    {
        var categories = await _context.Categories.ToListAsync();
        var categoryDtos = _mapper.Map<IEnumerable<GetCategoryDto>>(categories);
        return Ok(categoryDtos);
    }
}
