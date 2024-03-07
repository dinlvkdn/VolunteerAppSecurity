using VolunteerAppSecurity.DTOs;

namespace VolunteerAppSecurity.Interfaces
{
    public interface IUserService
    {
        public Task<UserDTO> CreateUser(RegisterDTO userDTO);
        public Task<UserDTO> GetUserById(string id);
        Task<bool> VerifyEmail(UserDTO userDTO, string code);
        public Task<bool> UserExist(string email);
        public Task<bool> DeleteUser(string email);
    }
}
