namespace VolunteerAppSecurity.Interfaces
{
    public class UserAuthenticationResponse
    {
        public string RefreshToken { get; set; }
        public string AccessToken { get; set; }
    }
}
