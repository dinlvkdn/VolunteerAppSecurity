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
        public Task<UserDTO> GetUserByName(string name);
        Task<bool> VerifyEmail(UserDTO userDTO, string code);
        public Task<bool> UserExist(string email);
        public Task<bool> DeleteUser(string email);
        public Task<AuthenticationResponse> GenerateTokens(string email);
        Task<bool> SendPasswordResetToken(UserDTO userDTO);
        Task<bool> ConfirmPasswordReset(UserDTO userDTO, string password, string newPassword);
        Task<bool> CheckPassword(string email, string password);
    }
}
