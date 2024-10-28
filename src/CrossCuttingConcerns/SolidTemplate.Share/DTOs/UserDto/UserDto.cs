namespace SolidTemplate.Share.DTOs.UserDto;

public class UserDto : BaseDto
{
    public bool IsAuthenticated { get; set; }
    public Guid UserId { get; set; }
    public string? UserName { get; set; }
    public string TenantId { get; set; } = default!;
    public string? Email { get; set; }
    public string FirstName { get; set; } = default!;
    public string LastName { get; set; } = default!;
    public bool HasPassword { get; set; } = default!;
    public string PhoneNumber { get; set; } = default!;
    public bool TwoFactorEnabled { get; set; }
    public bool HasAuthenticator { get; set; }
    public List<KeyValuePair<string, string>> Logins { get; set; } = default!;
    public bool BrowserRemembered { get; set; }
    public string SharedKey { get; set; } = default!;
    public string AuthenticatorUri { get; set; } = default!;
    public string[] RecoveryCodes { get; set; } = default!;
    public int CountRecoveryCodes { get; set; }
    public List<string>? Roles { get; set; }
    public List<KeyValuePair<string, string>> ExposedClaims { get; set; } = default!;
}
