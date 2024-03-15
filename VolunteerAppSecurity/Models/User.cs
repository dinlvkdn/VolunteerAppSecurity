using Microsoft.AspNetCore.Identity;

namespace VolunteerAppSecurity.Models
{
    public class User : IdentityUser<Guid>
    {
        public string? RefreshToken { get; set; }
        public bool IsDeleted { get; set; }
    }
}
