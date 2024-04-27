using Microsoft.AspNetCore.Identity;
using VolunteerAppSecurity.Models;
using VolunteerAppSecurity.Models.Enums;

namespace VolunteerAppSecurity
{
    public class DataSeeder
    {
        private readonly UserManager<User> _userManager;
        private readonly RoleManager<IdentityRole<Guid>> _roleManager;

        public DataSeeder(UserManager<User> userManager, RoleManager<IdentityRole<Guid>> roleManager)
        {
            _userManager = userManager;
            _roleManager = roleManager;
        }

        public async Task SeedRoles()
        {
            if (!await _roleManager.RoleExistsAsync(Role.Admin.ToString()))
                await _roleManager.CreateAsync(new IdentityRole<Guid>(Role.Admin.ToString()));
        }

        public async Task SeedAdmins()
        {
            var adminId = Guid.NewGuid();

            string email = "nlvkdn911@yopmail.com";
            string password = "Diana2004*";

            if (await _userManager.FindByIdAsync(adminId.ToString()) == null)
            {
                var admin = new User()
                {
                    Id = adminId,
                    UserName = email,
                    Email = email,
                };

                var result = await _userManager.CreateAsync(admin, password);

                if (result.Succeeded)
                {
                    await _userManager.AddToRoleAsync(admin, Role.Admin.ToString());
                }
            }
        }  
    }
}
