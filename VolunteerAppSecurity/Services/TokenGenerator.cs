using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using VolunteerAppSecurity.Interfaces;
using VolunteerAppSecurity.Models;

namespace VolunteerAppSecurity.Services
{
    public class TokenGenerator : ITokenGenerator
    {
        private const int RefreshTokenSize = 32;
        private readonly UserManager<User> _userManager;
        private readonly AuthenticationSetup _authSettings;
        public TokenGenerator(UserManager<User> userManager, IOptions<AuthenticationSetup> authSettings)
        {
            _userManager = userManager;
            _authSettings = authSettings.Value;
        }

        public async Task<string> GenerateAccessToken(User user)
        {
            var handler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(_authSettings.SecretKey);
            var roles = await _userManager.GetRolesAsync(user);


            ClaimsIdentity identity = new ClaimsIdentity(new[] {
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Role, roles.FirstOrDefault(ClaimTypes.Role)),
                new Claim(ClaimTypes.Name, user.UserName)
            });

            var securityToken = handler.CreateToken(new SecurityTokenDescriptor
            {
                Issuer = _authSettings.Issuer,
                Audience = _authSettings.Audience,
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature),
                Subject = identity,
                Expires = DateTime.Now.AddHours(_authSettings.AccessTokenExpirationMinutes)
            });
            return handler.WriteToken(securityToken);
        }

        public async Task<AuthenticationResponse> GenerateTokens(User user)
        {
            user.RefreshToken = GenerateRefreshToken(user);
            var result = await _userManager.UpdateAsync(user);
            if (!result.Succeeded)
            {
                throw new Exception("Unable to create refresh token");
            }
            return new AuthenticationResponse
            {
                AccessToken = await GenerateAccessToken(user),
                RefreshToken = user.RefreshToken,
            };
        }

        public string GenerateRefreshToken(User user)
        {
            var randomNumber = new byte[RefreshTokenSize];
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(randomNumber);
                return Convert.ToBase64String(randomNumber);
            }
        }

        public async Task<AuthenticationResponse> RefreshAccessToken(string accessToken, string refreshToken)
        {
            var principal = GetPrincipalFromExpiredToken(accessToken);
            var user = await _userManager.FindByNameAsync(principal.Identity.Name);
            if (user == null || user.RefreshToken != refreshToken)
            {
                throw new SecurityTokenException("Invalid refresh token");
            }

            user.RefreshToken = GenerateRefreshToken(user);
            var result = await _userManager.UpdateAsync(user);
            if (!result.Succeeded)
            {
                throw new Exception("Unable to create refresh token");
            }

            return new AuthenticationResponse
            {
                AccessToken = await GenerateAccessToken(user),
                RefreshToken = user.RefreshToken
            };
        }


        private ClaimsPrincipal GetPrincipalFromExpiredToken(string token)
        {
            var tokenValidationParameters = new TokenValidationParameters
            {
                ValidateAudience = false,
                ValidateIssuer = false,
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = _authSettings.GetSecurityKey(),
                ValidateLifetime = false
            };

            var tokenHandler = new JwtSecurityTokenHandler();
            var principal = tokenHandler.ValidateToken(token, tokenValidationParameters, out _);

            return principal;
        }
    }
}
