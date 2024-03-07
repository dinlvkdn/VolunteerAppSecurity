using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using VolunteerAppSecurity.DTOs;
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
        readonly UserManager<User> _userManager;

        public UserController(IUserService userService, ITokenGenerator tokenGenerator, UserManager<User> userManager)
        {
            _userService = userService;
            _tokenGenerator = tokenGenerator;
            _userManager = userManager;
        }

        [HttpPost]
        [AllowAnonymous]
        public async Task<IActionResult> Login([FromBody] LoginDTO userLoginDTO)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var user = await _userManager.FindByEmailAsync(userLoginDTO.Email);

            if (user == null) return NotFound("No user exists");

            var token = await _tokenGenerator.GenerateTokens(user);

            if (token == null)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, "Error occured while creating user on server");
            }
            
            return Ok(token);
        }

        [HttpPost]
        [AllowAnonymous]
        public async Task<IActionResult> Registration([FromBody] RegisterDTO userRegisterDTO)
        {
            try
            {
                if (!ModelState.IsValid) return BadRequest(ModelState);

                var user = await _userManager.FindByEmailAsync(userRegisterDTO.Email);
                var isUserExists = await _userService.UserExist(userRegisterDTO.Email);

                if (isUserExists)
                {
                    return BadRequest("A user with this email already exists");
                }
                var createdUser = await _userService.CreateUser(userRegisterDTO);
                if (createdUser != null) return Created("/api/user", user);

            }
            catch (Exception) { }

            return StatusCode(StatusCodes.Status500InternalServerError, "Error occured while creating user on server");
        }

        [HttpGet]
        public async Task<IActionResult> VerificationEmail([FromQuery] string userId, [FromQuery] string code)
        {
            if (userId == null || code == null)
                return BadRequest(new AuthResponse()
                {
                    Errors = new List<string>() {"Invalid email confirmation url"},
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
    }
}