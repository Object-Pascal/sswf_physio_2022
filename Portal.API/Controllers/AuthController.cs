using Core.Domain;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SignInResult = Microsoft.AspNetCore.Identity.SignInResult;

namespace Portal.API.Controllers
{
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly ILogger<AuthController> _logger;
        private readonly SignInManager<IdentityUser> _signInManager;
        private readonly UserManager<IdentityUser> _userManager;

        public AuthController(ILogger<AuthController> logger, UserManager<IdentityUser> userManager, SignInManager<IdentityUser> signInManager)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _signInManager = signInManager ?? throw new ArgumentNullException(nameof(signInManager));
            _userManager = userManager ?? throw new ArgumentNullException(nameof(userManager));
        }

        [HttpPost("api/v1/auth/signin")]
        public async Task<IActionResult> SignIn([FromBody] SignInRequest body)
        {
            IdentityUser identityUser = await _userManager.FindByNameAsync(body.UserName);
            if (identityUser != null)
            {
                SignInResult signInResult = await _signInManager.PasswordSignInAsync(identityUser, body.Password, false, false);
                if (signInResult.Succeeded)
                {
                    SecurityTokenDescriptor securityTokenDescriptor = new SecurityTokenDescriptor()
                    {
                        Subject = (await _signInManager.CreateUserPrincipalAsync(identityUser)).Identities.First(),
                        Expires = DateTime.Now.AddHours(3),
                        SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(Encoding.UTF8.GetBytes("701f6335-e6c0-4bb4-866c-e08ef2328b9c")), SecurityAlgorithms.HmacSha256Signature)
                    };
                    return Ok(new { Token = new JwtSecurityTokenHandler().WriteToken(new JwtSecurityTokenHandler().CreateToken(securityTokenDescriptor)) });
                }
            }
            return Unauthorized();
        }
    }
}