using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;
namespace SolidTemplate.Domain.DataModels;

public class ApplicationUser : IdentityUser
{
    [MaxLength(64)]
    public string FirstName { get; set; } = string.Empty;

    [MaxLength(64)]
    public string LastName { get; set; } = string.Empty;

    public string FullName => $"{FirstName} {LastName}";
}
