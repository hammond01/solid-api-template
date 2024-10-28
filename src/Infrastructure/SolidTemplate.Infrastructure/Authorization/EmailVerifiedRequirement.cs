namespace SolidTemplate.Infrastructure.Authorization;

public class EmailVerifiedRequirement : IAuthorizationRequirement
{

    public EmailVerifiedRequirement(bool isEmailVerified)
    {
        IsEmailVerified = isEmailVerified;
    }
    public bool IsEmailVerified { get; private set; }//not used
}
public class EmailVerifiedHandler : AuthorizationHandler<EmailVerifiedRequirement>
{
    protected override Task HandleRequirementAsync(AuthorizationHandlerContext context,
        EmailVerifiedRequirement requirement)
    {
        if (!context.User.HasClaim(c => c.Type == ApplicationClaimTypes.EmailVerified))
        {
            return Task.CompletedTask;
        }
        {
            var claim = context.User.FindFirst(c => c.Type == ApplicationClaimTypes.EmailVerified);
            var isEmailVerified = Convert.ToBoolean(claim!.Value);

            if (isEmailVerified)
            {
                context.Succeed(requirement);
            }
        }

        return Task.CompletedTask;
    }
}
