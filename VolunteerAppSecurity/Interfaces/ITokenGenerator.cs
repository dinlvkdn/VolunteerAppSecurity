using VolunteerAppSecurity.Models;
using VolunteerAppSecurity.Services;

namespace VolunteerAppSecurity.Interfaces
{
    public interface ITokenGenerator
    {
        Task<AuthenticationResponse> GenerateTokens(User user);
    }
}
