using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using MyBudget.Core.Dtos.Users;
using MyBudget.Core.Models;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace MyBudget.Core.Controllers
{
    [Route("api/users")]
    [ApiController]
    public class UsersController : ControllerBase
    {
        private readonly UserManager<ApplicationUser> _userManager;

        private readonly SignInManager<ApplicationUser> _signInManager;

        private readonly ILogger<UsersController> _logger;

        private readonly IConfiguration _configuration;

        public UsersController(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            ILogger<UsersController> logger,
            IConfiguration configuration
        ) {
            _userManager = userManager;
            _signInManager = signInManager;
            _logger = logger;
            _configuration = configuration;
        }

        [HttpGet]
        public async Task<ActionResult<string>> GetAsync()
        {
            return await Task.FromResult<string>(DateTimeOffset.UtcNow.ToString());
        }

        [HttpPost("create")]
        public async Task<ActionResult<UserTokenDto>> CreateAsync([FromBody] UserInfoDto dto)
        {
            var user = new ApplicationUser() { UserName = dto.Email, Email = dto.Email };
            var result = await _userManager.CreateAsync(user, dto.Password);

            if (result.Succeeded)
            {
                _logger.LogInformation("Usuario '{0}' criado na aplicação", dto.Email);
                var token = BuildToken(dto);
                return Created($"", token);
            }
            else
            {
                foreach (var error in result.Errors)
                    ModelState.AddModelError(error.Code, error.Description);

                return BadRequest(ModelState);
            }
        }

        [HttpPost("login")]
        public async Task<ActionResult<UserTokenDto>> LoginAsync([FromBody] UserInfoDto dto)
        {
            var result = await _signInManager.PasswordSignInAsync(dto.Email, dto.Password,
                 isPersistent: true, lockoutOnFailure: true);

            if (result.Succeeded)
            {
                _logger.LogInformation("Usuario '{0}' entrou na aplicação", dto.Email);
                return BuildToken(dto);
            }
            else
            {
                _logger.LogInformation("Usuario '{0}' tentou entrar na aplicação", dto.Email);
                ModelState.AddModelError(string.Empty, "Usuário ou senha inválido!");
                return BadRequest(ModelState);
            }
        }   

        private UserTokenDto BuildToken(UserInfoDto userInfo)
        {
            var claims = new[]
            {
                new Claim(JwtRegisteredClaimNames.UniqueName, userInfo.Email),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            // tempo de expiração do token: 1 hora
            var expiration = DateTime.UtcNow.AddHours(1);
            JwtSecurityToken token = new JwtSecurityToken(
               issuer: null,
               audience: null,
               claims: claims,
               expires: expiration,
               signingCredentials: creds);

            return new UserTokenDto()
            {
                Token = new JwtSecurityTokenHandler().WriteToken(token),
                Expiration = expiration
            };
        }
    }
}
