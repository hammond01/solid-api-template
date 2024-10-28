// using modified example from https://chrissainty.com/securing-your-blazor-apps-configuring-policy-based-authorization-with-blazor/
namespace SolidTemplate.Infrastructure.Authorization;

public class DomainRequirement : IAuthorizationRequirement
{

    public DomainRequirement(string requiredDomain)
    {
        RequiredDomain = requiredDomain;
    }
    public string RequiredDomain { get; }
}
public class DomainRequirementHandler : AuthorizationHandler<DomainRequirement>
{
    protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, DomainRequirement requirement)
    {
        if (!context.User.HasClaim(c => c.Type == ClaimTypes.Email))
        {
            return Task.CompletedTask;
        }

        var emailAddress = context.User.FindFirst(c => c.Type == ClaimTypes.Email)!.Value;

        if (emailAddress.ToLower().EndsWith(requirement.RequiredDomain.ToLower()))// includes subdomains
        {
            context.Succeed(requirement);
        }

        // you can add as many checks as your want for a given policy
        return Task.CompletedTask;
    }
}
