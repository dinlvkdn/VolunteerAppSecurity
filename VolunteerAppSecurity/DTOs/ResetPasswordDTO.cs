namespace VolunteerAppSecurity.DTOs
{
    public class ResetPasswordDTO
    {
        public string Email { get; set; }
        public string NewPassword { get; set; }
        public string ResetCode { get; set; }

    }
}
