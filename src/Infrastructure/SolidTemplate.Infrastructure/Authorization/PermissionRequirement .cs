namespace SolidTemplate.Infrastructure.Authorization;

public class PermissionRequirement : IAuthorizationRequirement
{
    public PermissionRequirement(string permission)
    {
        Permission = permission;
    }

    public string Permission { get; set; }
}
public class PermissionRequirementHandler : AuthorizationHandler<PermissionRequirement>, IAuthorizationRequirement
{
    protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, PermissionRequirement requirement)
    {
        if (context.User.Claims.Any(c => c.Type == ApplicationClaimTypes.Permission && c.Value == requirement.Permission))
        {
            context.Succeed(requirement);
        }
        else
        {
            context.Fail();
        }
        return Task.CompletedTask;
    }
}
