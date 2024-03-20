using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using VolunteerAppSecurity.DTOs;
using VolunteerAppSecurity.Exceptions;
using VolunteerAppSecurity.Interfaces;
using VolunteerAppSecurity.Models;
using VolunteerAppSecurity.Services;

namespace VolunteerAppSecurity.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        readonly IUserService _userService;
        readonly ITokenGenerator _tokenGenerator;

        public UserController(IUserService userService, ITokenGenerator tokenGenerator)
        {
            _userService = userService;
            _tokenGenerator = tokenGenerator;
        }

        [HttpPost("login")]
        [AllowAnonymous]
        public async Task<IActionResult> Login([FromBody] LoginDTO userLoginDTO)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var user = await _userService.UserExist(userLoginDTO.Email);
            if (!user) return NotFound("No user exists");

            var checkPassword = await _userService.CheckPassword(userLoginDTO.Email, userLoginDTO.Password);

            if (!checkPassword) return BadRequest("Incorrect password!");

            var token = await _userService.GenerateTokens(userLoginDTO.Email);

            if (token == null)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, "Error occured while creating user on server");
            }

            return Ok(token);
        }

        [HttpPost("register")]
        [AllowAnonymous]
        public async Task<IActionResult> Registration([FromBody] RegisterDTO userRegisterDTO)
        {
            if (!ModelState.IsValid)
                throw new ApiException()
                {
                    StatusCode = StatusCodes.Status404NotFound,
                    Title = "Not found",
                    Detail = "User doesn't exist"
                };

            var createdUser = await _userService.CreateUser(userRegisterDTO);

            if (createdUser != null)
                return Created("/api/user", createdUser);
            else
                throw new ApiException()
                {
                    StatusCode = StatusCodes.Status500InternalServerError,
                    Title = "Server error",
                    Detail = "Error occured while creating user on server"
                };
        }

        [HttpGet("verification")]
        public async Task<IActionResult> VerificationEmail([FromQuery] string userId, [FromQuery] string code)
        {
            if (userId == null || code == null)
                return BadRequest(new AuthResponse()
                {
                    Errors = new List<string>() { "Invalid email confirmation url" },
                    Result = false
                });

            var user = await _userService.GetUserById(userId);

            if (user == null)
            {
                return BadRequest(new AuthResponse()
                {
                    Errors = new List<string>()
                    {
                        "Invalid email parameter"
                    },
                    Result = false
                });
            }

            code = code.Replace(' ', '+');

            if (await _userService.VerifyEmail(user, code))
            {
                return Content(Constants.SuccessMessage, "text/html");
            }
            else
            {
                return Content(Constants.FailureMessage, "text/html");
            }
        }

        [AllowAnonymous]
        [HttpPost("token")]
        public async Task<IActionResult> RefreshToken([FromBody] RefreshTokenDTO refreshTokenDTO)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var result = await _tokenGenerator.RefreshAccessToken(refreshTokenDTO.AccessToken,
                refreshTokenDTO.RefreshToken);

            if (result == null) return StatusCode(StatusCodes.Status400BadRequest);

            return Ok(result);
        }

        [AllowAnonymous]
        [HttpPost("resetcode")]
        public async Task<IActionResult> ResettingCode([FromBody] EmailDTO emailDTO)
        {
            var id = HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier);
            var user = await _userService.UserExist(emailDTO.To);

            if (!user) return BadRequest("User doesn't exists");

            var userGetResetCode = await _userService.GetUserById(id);

            if (await _userService.SendPasswordResetToken(userGetResetCode))
            {
                return Ok("Reset code has sent!");
            }
            else
            {
                return StatusCode(StatusCodes.Status500InternalServerError, "Server error");
            }
        }

        [AllowAnonymous]
        [HttpPost("verifyresetcode")]
        public async Task<IActionResult> VerifyingResetCode([FromBody] ResetPasswordDTO resetPasswordDTO)
        {
            if (!await _userService.UserExist(resetPasswordDTO.Email))
            {
                return BadRequest("Invalid email or doesn't exists");
            }

            var user = await _userService.GetUserByEmail(resetPasswordDTO.Email);
            if (await _userService.ConfirmPasswordReset(user, resetPasswordDTO.ResetCode, resetPasswordDTO.NewPassword))
            {
                return Ok("Password reset successfully");
            }
            else
            {
                return StatusCode(StatusCodes.Status500InternalServerError, "Server error");
            }
        }

        [Authorize]
        [HttpDelete]
        public async Task<IActionResult> DeleteUser([FromQuery] Guid userId)
        {
            var deletedUser = await _userService.DeleteUserById(userId);

            if (deletedUser)
            {
                return Ok();
            }
            else
            {
                throw new ApiException()
                {
                    StatusCode = StatusCodes.Status500InternalServerError,
                    Title = "Server error",
                    Detail = "Error occured while creating user on server"
                };
            }
        }
    }
}