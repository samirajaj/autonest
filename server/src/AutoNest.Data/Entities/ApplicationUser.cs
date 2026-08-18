using Microsoft.AspNetCore.Identity;

namespace AutoNest.Data.Entities;

public sealed class ApplicationUser : IdentityUser
{
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
