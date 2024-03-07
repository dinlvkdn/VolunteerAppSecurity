using Moq;
using VolunteerAppSecurity.DTOs;
using VolunteerAppSecurity.Interfaces;
using VolunteerAppSecurity.Services;

namespace VolnteerAppSecurity.Tests.UserControllerTest
{
    public class RegisterUserTest
    {
        private Mock<IUserService, UserService> _mockUserService;
        private Mock<ITokenGenerator, TokenGenerator> _mockTokenGenerator;
        [SetUp]
        public void SetUp()
        {
            _mockTokenGenerator = new Mock<ITokenGenerator>();
            _mockUserService = new Mock<IUserService>();
        }

        [TearDown]
        public void TearDown()
        {

        }
        public void SuccessRegisterTest() {
            var user = new RegisterDTO
            {
                UserName = "Diana",
                Email = "nalivaykodiana@gmail.com",
                Password = "fgkdjadua75wajehqj5",
                RoleName = "Volunteer"

            };

            var us = new RegisterDTO
            {
                UserName = "Diana",
                Email = "nalivaykodiana@gmail.com",
                Password = "fgkdjadua75wajehqj5",
                RoleName = "Volunteer"
            };

            _mockUserService.Setup(x => x.FindByEmailAsync(RegisterDTO.Email)).ReturnsAsync(us));
        };

    }
}
