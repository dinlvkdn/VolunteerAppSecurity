using Microsoft.IdentityModel.Tokens;
using System.Text;

namespace VolunteerAppSecurity.Services
{
    public class AuthenticationSetup
    {
        public string SecretKey {  get; set; }
        public double AccessTokenExpirationMinutes {  get; set; }
        public string Issuer {  get; set; }
        public string Audience {  get; set; }

        public SecurityKey GetSecurityKey()
        {
            return new SymmetricSecurityKey(Encoding.ASCII.GetBytes(SecretKey));
        }
    }
}
