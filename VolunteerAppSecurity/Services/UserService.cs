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
            var callbackUrl = ngrok + "/api/User/VerificateEmail" + $"?userId={user.Id}&code={code}";

            return Task.FromResult(callbackUrl);
        }

        public Task<bool> UserExist(string email)
            => _securityDBContext.Users.AnyAsync(u => u.Email == email);

        public async Task<UserDTO> CreateUser(RegisterDTO registerDTO)
        {
            var checkByEmail = await UserExist(registerDTO.Email);
            var checkByUserName = await _securityDBContext.Users.AnyAsync(u => u.UserName == registerDTO.UserName); ;
            if (checkByEmail) 
            {
                throw new ApiException()
                {
                    StatusCode = StatusCodes.Status400BadRequest,
                    Title = "User exist",
                    Detail = "User with this email already exists"
                };
            }
            if (checkByUserName)
            {
                throw new ApiException()
                {
                    StatusCode = StatusCodes.Status400BadRequest,
                    Title = "User exist",
                    Detail = "User with this username already exists"
                };
            }

            var user = new User()
            {
                UserName = registerDTO.UserName,
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
                        UserName = foundUser.UserName,
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
                        Detail = "User doesn't created"
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
                UserName = user.UserName
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
                    Text = $@"
<!DOCTYPE html>
<html>
<head>
  <meta charset=""utf-8"">
  <meta http-equiv=""x-ua-compatible"" content=""ie=edge"">
  <title>Email Confirmation</title>
  <meta name=""viewport"" content=""width=device-width, initial-scale=1"">
  <style type=""text/css"">
    @media screen {{
      @font-face {{
        font-family: 'Source Sans Pro';
        font-style: normal;
        font-weight: 400;
        src: local('Source Sans Pro Regular'), local('SourceSansPro-Regular'), url(https://fonts.gstatic.com/s/sourcesanspro/v10/ODelI1aHBYDBqgeIAH2zlBM0YzuT7MdOe03otPbuUS0.woff) format('woff');
      }}
      @font-face {{
        font-family: 'Source Sans Pro';
        font-style: normal;
        font-weight: 700;
        src: local('Source Sans Pro Bold'), local('SourceSansPro-Bold'), url(https://fonts.gstatic.com/s/sourcesanspro/v10/toadOcfmlt9b38dHJxOBGFkQc6VGVFSmCnC_l7QZG60.woff) format('woff');
      }}
    }}
    body,
    table,
    td,
    a {{
      -ms-text-size-adjust: 100%; /* 1 */
      -webkit-text-size-adjust: 100%; /* 2 */
    }}
    table,
    td {{
      mso-table-rspace: 0pt;
      mso-table-lspace: 0pt;
    }}
    img {{
      -ms-interpolation-mode: bicubic;
    }}
    a[x-apple-data-detectors] {{
      font-family: inherit !important;
      font-size: inherit !important;
      font-weight: inherit !important;
      line-height: inherit !important;
      color: inherit !important;
      text-decoration: none !important;
    }}
    div[style*=""margin: 16px 0;""] {{
      margin: 0 !important;
    }}
    body {{
      width: 100% !important;
      height: 100% !important;
      padding: 0 !important;
      margin: 0 !important;
    }}
    table {{
      border-collapse: collapse !important;
    }}
    a {{
      color: #1a82e2;
    }}
    img {{
      height: auto;
      line-height: 100%;
      text-decoration: none;
      border: 0;
      outline: none;
    }}
  </style>
</head>
<body style=""background-color: #e9ecef;"">
  <div class=""preheader"" style=""display: none; max-width: 0; max-height: 0; overflow: hidden; font-size: 1px; line-height: 1px; color: #fff; opacity: 0;"">
    A preheader is the short summary text that follows the subject line when an email is viewed in the inbox.
  </div>
  <table border=""0"" cellpadding=""0"" cellspacing=""0"" width=""100%"">
    <tr>
      <td align=""center"" bgcolor=""#e9ecef"">
        <table border=""0"" cellpadding=""0"" cellspacing=""0"" width=""100%"" style=""max-width: 600px;"">
          <tr>
            <td align=""center"" valign=""top"" style=""padding: 36px 24px;"">
              <img src=""https://www.blogdesire.com/wp-content/uploads/2019/07/blogdesire-1.png"" alt=""Logo"" border=""0"" width=""48"" style=""display: block; width: 48px; max-width: 48px; min-width: 48px;"">
            </td>
          </tr>
        </table>
      </td>
    </tr>
    <tr>
      <td align=""center"" bgcolor=""#e9ecef"">
        <table border=""0"" cellpadding=""0"" cellspacing=""0"" width=""100%"" style=""max-width: 600px;"">
          <tr>
            <td align=""left"" bgcolor=""#ffffff"" style=""padding: 36px 24px 0; font-family: 'Source Sans Pro', Helvetica, Arial, sans-serif; border-top: 3px solid #d4dadf;"">
              <h1 style=""margin: 0; font-size: 32px; font-weight: 700; letter-spacing: -1px; line-height: 48px;"">Confirm Your Email  Address</h1>
            </td>
          </tr>
        </table>
      </td>
    </tr>
    <tr>
      <td align=""center"" bgcolor=""#e9ecef"">
        <table border=""0"" cellpadding=""0"" cellspacing=""0"" width=""100%"" style=""max-width: 600px;"">
          <tr>
            <td align=""left"" bgcolor=""#ffffff"" style=""padding: 24px; font-family: 'Source Sans Pro', Helvetica, Arial, sans-serif; font-size: 16px; line-height: 24px;"">
              <p style=""margin: 0;"">To complete the registration process and activate your account, we need to confirm your email address. Please click the button below to confirm your email address. If you did not register on our site, it may have been a mistake. In this case, you can ignore this message. Thank you!</p>
            </td>
          </tr>
          <tr>
            <td align=""left"" bgcolor=""#ffffff"">
              <table border=""0"" cellpadding=""0"" cellspacing=""0"" width=""100%"">
                <tr>
                  <td align=""center"" bgcolor=""#ffffff"" style=""padding: 12px;"">
                    <table border=""0"" cellpadding=""0"" cellspacing=""0"">
                      <tr>
                        <td align=""center"" bgcolor=""#1a82e2"" style=""border-radius: 6px;"">
                            <a href=""{emailConfirmationUrl}"" class=""verify-link"" style=""display: inline-block; padding: 16px 36px; font-family: 'Source Sans Pro', Helvetica, Arial, sans-serif; font-size: 16px; color: #ffffff; text-decoration: none; border-radius: 6px;"">Confirm email</a>
                        </td>
                      </tr>
                    </table>
                  </td>
                </tr>
              </table>
            </td>
          </tr>
        </table>
      </td>
    </tr>
  </table>
</body>
</html>"
                
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
            var userVerify = new User()
            {
                Email = user.Email,
            };
            var result = await _userManager.ConfirmEmailAsync(userVerify, token);

            return result.Succeeded;
        }

        private async Task<string> AddUserRoleAsync(User user, string roleName)
        {
            var isAddedUserRole = await _userManager.AddToRoleAsync(user, roleName);
         
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

        public async Task<bool> DeleteUser(string email)
        {
            var foundUser = await _userManager.FindByEmailAsync(email);
            if (foundUser != null)
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

        public async Task<bool> SendPasswordResetToken(UserDTO userDTO)
        {
            var user = new User()
            {
                Email = userDTO.Email,
            };

            var resetToken = await _userManager.GeneratePasswordResetTokenAsync(user);

            var ngrok = Constants.ngrok;
            var callbackUrl = ngrok + "/api/User/VerifyPasswordResetCode" + $"?userId={userDTO.Id}&code={resetToken}";

            try
            {
                var email = new MimeMessage();
                email.From.Add(MailboxAddress.Parse(Constants.SenderEmail));
                email.To.Add(MailboxAddress.Parse(user.Email));
                email.Subject = "Reset Password";
                email.Body = new TextPart(TextFormat.Html)
                {
                    Text = $@"<html lang=""en"">
                            <head>
                              <meta charset=""UTF-8"">
                              <meta name=""viewport"" content=""width=device-width, initial-scale=1.0"">
                              <title><3</title>
                            </head>
                            <body>
                            <h1> Колись тут буде відновлення паролю <3 </h1>
                            </body>
                            </html>
                    "
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
            catch (Exception) { return false; }
        }

        public async Task<bool> ConfirmPasswordReset(UserDTO userDTO, string password, string newPassword)
        {
            var user = new User()
            {
                Email = userDTO.Email
            };

            var result = await _userManager.ResetPasswordAsync(user, password, newPassword);
            return result.Succeeded;
        }

        public async Task<bool> CheckPassword(string email, string password)
        {
            var userByEmail = await _userManager.FindByEmailAsync(email);
            return await _userManager.CheckPasswordAsync(userByEmail, password);
        }

    }

}
