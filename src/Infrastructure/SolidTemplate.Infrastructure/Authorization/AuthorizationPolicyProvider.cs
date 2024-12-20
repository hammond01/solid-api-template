﻿using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Options;
namespace SolidTemplate.Infrastructure.Authorization;

public class AuthorizationPolicyProvider : SharedAuthorizationPolicyProvider
{
    private readonly AuthorizationOptions _options;

    public AuthorizationPolicyProvider(IOptions<AuthorizationOptions> options) : base(options)
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
        policy = new AuthorizationPolicyBuilder()
            .RequireAuthenticatedUser()
            .AddRequirements(new PermissionRequirement(policyName))
            .Build();
        _options.AddPolicy(policyName, policy);
        return policy;
    }
}
