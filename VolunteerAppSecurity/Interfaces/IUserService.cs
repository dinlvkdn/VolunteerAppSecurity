using VolunteerAppSecurity.DTOs;
using VolunteerAppSecurity.Models;
using VolunteerAppSecurity.Services;

namespace VolunteerAppSecurity.Interfaces
{
    public interface IUserService
    {
        public Task<UserDTO> CreateUser(RegisterDTO userDTO);
        public Task<UserDTO> GetUserById(string id);
        public Task<UserDTO> GetUserByEmail(string email);
        Task<bool> VerifyEmail(UserDTO userDTO, string code);
        public Task<bool> UserExist(string email);
        public Task<bool> DeleteUserById(Guid id);
        public Task<AuthenticationResponse> GenerateTokens(string email);
        Task<bool> CheckPassword(string email, string password);
    }
}
