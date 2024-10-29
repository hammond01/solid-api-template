using SolidTemplate.Shared;
namespace SolidTemplate.API.Middleware;

public class LocalizationMiddleware
{
    private readonly RequestDelegate _next;

    public LocalizationMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var cultureQuery = context.Request.Query["culture"].ToString();

        if (!string.IsNullOrEmpty(cultureQuery))
        {
            CultureInfoManager.SetCurrentCulture(cultureQuery);
        }
        else
        {
            context.Request.Cookies.TryGetValue("SetCulture", out var culture);
            if (culture != null)
                CultureInfoManager.SetCurrentCulture(culture);
        }
        await _next(context);
    }
}
