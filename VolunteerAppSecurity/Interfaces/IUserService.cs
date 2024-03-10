using VolunteerAppSecurity.DTOs;
using VolunteerAppSecurity.Models;

namespace VolunteerAppSecurity.Interfaces
{
    public interface IUserService
    {
        public Task<bool> CreateUser(RegisterDTO userDTO);
        public Task<User> GetUserById(string id);
        Task<bool> VerifyEmail(User user, string code);
        Task<string> CallBackUrl(User user, string code);
        Task<bool> SendEmail(User user);
    }
}
