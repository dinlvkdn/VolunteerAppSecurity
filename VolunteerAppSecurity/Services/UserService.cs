using MailKit.Security;
using Microsoft.AspNetCore.Identity;
using MimeKit.Text;
using MimeKit;
using VolunteerAppSecurity.DTOs;
using VolunteerAppSecurity.Interfaces;
using VolunteerAppSecurity.Models;
using MailKit.Net.Smtp;
using Microsoft.EntityFrameworkCore;
using VolunteerAppSecurity.DataAccess;
using VolunteerAppSecurity.Exceptions;
using static Org.BouncyCastle.Crypto.Engines.SM2Engine;
using VolunteerAppSecurity.Helpers;
using VolunteerAppSecurity.Models.Enums;

namespace VolunteerAppSecurity.Services
{
    public class UserService : IUserService
    {
        readonly UserManager<User> _userManager;
        readonly RoleManager<IdentityRole<Guid>> _roleManager;
        private readonly SecurityDBContext _securityDBContext;
        private readonly ITokenGenerator _tokenGenerator;
        public UserService(UserManager<User> userManager, RoleManager<IdentityRole<Guid>> roleManager, SecurityDBContext securityDBContext, ITokenGenerator tokenGenerator)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _securityDBContext = securityDBContext;
            _tokenGenerator = tokenGenerator;
        }

        public Task<string> CallBackUrl(User user, string code)
        {
            var ngrok = Constants.ngrok;
            var callbackUrl = ngrok + "/api/User/verification" + $"?userId={user.Id}&code={code}";

            return Task.FromResult(callbackUrl);
        }

        public Task<bool> UserExist(string email)
            => _securityDBContext.Users.AnyAsync(u => u.Email == email);

        public async Task<UserDTO> CreateUser(RegisterDTO registerDTO)
        {
            var checkByEmail = await UserExist(registerDTO.Email);
            if (checkByEmail) 
            {
                throw new ApiException()
                {
                    StatusCode = StatusCodes.Status400BadRequest,
                    Title = "User exist",
                    Detail = "User with this email already exists"
                };
            }
      
            var user = new User()
            {
                UserName = registerDTO.Email,
                Email = registerDTO.Email
            };

            var createUserResult = await _userManager.CreateAsync(user, registerDTO.Password);

            if (createUserResult.Succeeded)
            {
                var foundUser = await _userManager.FindByEmailAsync(user.Email);
                var roleResult = await AddUserRoleAsync(foundUser, registerDTO.RoleName);

                var isSentEmail = await SendEmail(foundUser);

                if (isSentEmail)
                {
                    return new UserDTO()
                    {
                        Email = foundUser.Email,
                        RoleName = roleResult,
                        Id = foundUser.Id
                    };
                }
                else
                {
                    await _userManager.DeleteAsync(foundUser);
                    throw new ApiException()
                    {
                        StatusCode = StatusCodes.Status500InternalServerError,
                        Title = "Error creating user",
                        Detail = "Failed to send email notification."
                    };
                }
            }
            else
                throw new ApiException()
                {
                    StatusCode = StatusCodes.Status500InternalServerError,
                    Title = "Error creating user",
                    Detail = "User was not added due to an error on the server"
                };
        }

        public async Task<UserDTO> GetUserById(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            return new UserDTO()
            {
                Id = user.Id,
                Email = user.Email
            };
        }

        public async Task<UserDTO> GetUserByEmail(string email)
        {
            var user = await _securityDBContext.Users.FirstOrDefaultAsync(i => i.Email == email);
            return new UserDTO()
            {
                Id = user.Id,
                Email = user.Email,
            };
        }

        private async Task<bool> SendEmail(User user)
        {
            var emailConfirmationToken = await _userManager.GenerateEmailConfirmationTokenAsync(user);
            var emailConfirmationUrl = await CallBackUrl(user, emailConfirmationToken);
            try
            {
                var email = new MimeMessage();
                email.From.Add(MailboxAddress.Parse(Constants.SenderEmail));
                email.To.Add(MailboxAddress.Parse(user.Email));
                email.Subject = "Email verification";
                email.Body = new TextPart(TextFormat.Html)
                {
                    Text = EmailTemplateGenerator.GenerateEmailTemplate(emailConfirmationUrl)
                };

                using var client = new SmtpClient();
                string mySmptServerAddres = "smtp.gmail.com";
                int mySmptPort = 587;
                await client.ConnectAsync(mySmptServerAddres, mySmptPort, SecureSocketOptions.StartTls);
                await client.AuthenticateAsync(Constants.SenderEmail, Constants.Password);

                await client.SendAsync(email);
                await client.DisconnectAsync(true);
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public async Task<bool> VerifyEmail(UserDTO user, string token)
        {
            var userVerify = await _userManager.FindByEmailAsync(user.Email);
            var result = await _userManager.ConfirmEmailAsync(userVerify, token);

            return result.Succeeded;
        }

        private async Task<string> AddUserRoleAsync(User user, Role roleName)
        {
            var isAddedUserRole = await _userManager.AddToRoleAsync(user, roleName.ToString());
         
            if (isAddedUserRole.Succeeded)
            {
                var roles = await _userManager.GetRolesAsync(user);
                return roles.First();
            }
            else
            { 
                throw new ApiException()
                {
                    StatusCode = StatusCodes.Status500InternalServerError,
                    Title = "Error adding role",
                    Detail = "Role doesn't added"
                };
            }
        }

        public async Task<bool> DeleteUserById(Guid id)
        {
            string userId = id.ToString();
            var foundUser = await _userManager.FindByIdAsync(userId);

            if (foundUser == null)
            {
                throw new ApiException()
                {
                    StatusCode = StatusCodes.Status404NotFound,
                    Title = "User not found",
                    Detail = "User not found"
                };
            }
            var deletionResult = await _userManager.DeleteAsync(foundUser);
            return deletionResult.Succeeded;
        }

        public async Task<AuthenticationResponse> GenerateTokens(string email)
        {
            var user = await _userManager.FindByEmailAsync(email);
            return await _tokenGenerator.GenerateTokens(user);
        }

        public async Task<bool> CheckPassword(string email, string password)
        {
            var userByEmail = await _userManager.FindByEmailAsync(email);
            return await _userManager.CheckPasswordAsync(userByEmail, password);
        }
    }

}
