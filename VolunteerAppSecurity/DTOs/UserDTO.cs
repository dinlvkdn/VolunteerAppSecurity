
namespace VolunteerAppSecurity.DTOs
{
    public class UserDTO
    {
        public string Email { get; set; }
        public string RoleName { get; set; }
        public Guid Id { get; internal set; }
    }
}
