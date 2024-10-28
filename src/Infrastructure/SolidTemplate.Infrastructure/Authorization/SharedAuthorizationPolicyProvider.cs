using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Options;
using SolidTemplate.Constants.AuthorizationDefinitions;
namespace SolidTemplate.Infrastructure.Authorization;

public class SharedAuthorizationPolicyProvider : DefaultAuthorizationPolicyProvider
{
    private readonly AuthorizationOptions _options;

    protected SharedAuthorizationPolicyProvider(IOptions<AuthorizationOptions> options) : base(options)
    {
        _options = options.Value;
    }

    public override async Task<AuthorizationPolicy?> GetPolicyAsync(string policyName)
    {
        var policy = await base.GetPolicyAsync(policyName);

        if (policy != null)
        {
            return policy;
        }
        var created = false;
        switch (policyName)
        {
            //In DatabaseInitializer: await _userManager.AddClaimAsync(applicationUser, new Claim($"Is{role}", ClaimValues.trueString));
            case Policies.IsAdmin:
                policy = new AuthorizationPolicyBuilder()
                    .Combine((await GetPolicyAsync(Policies.IsUser))!)
                    .RequireClaim("IsAdministrator")
                    .Build();

                created = true;
                break;

            case Policies.IsUser:
                policy = new AuthorizationPolicyBuilder()
                    .RequireAuthenticatedUser()
                    .AddRequirements(new EmailVerifiedRequirement(true))
                    .Build();

                created = true;
                break;

            case Policies.TwoFactorEnabled:
                policy = new AuthorizationPolicyBuilder()
                    .RequireAuthenticatedUser()
                    .RequireClaim("amr", "mfa")
                    .Build();

                created = true;
                break;

            case Policies.IsMyEmailDomain:
                policy = new AuthorizationPolicyBuilder()
                    .RequireAuthenticatedUser()
                    .AddRequirements(new DomainRequirement("template.com"))
                    .Build();

                created = true;
                break;
        }

        if (created)
        {
            _options.AddPolicy(policyName, policy!);
        }

        return policy;
    }
}
