using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using VolunteerAppSecurity.Models;

namespace VolunteerAppSecurity.DataAccess
{
    public class SecurityDBContext : IdentityDbContext<User, IdentityRole<Guid>, Guid>
    {
        public SecurityDBContext(DbContextOptions options) : base(options){}

        protected SecurityDBContext(){}
    }
}
