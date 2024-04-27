using VolunteerAppSecurity.Models.Enums;

namespace VolunteerAppSecurity.DTOs
{
    public class RegisterDTO
    {
        public string Email { get; set; }
        public string UserName { get; set; }

        public string Password { get; set; }
        public Role RoleName { get; set; }
    }
}
