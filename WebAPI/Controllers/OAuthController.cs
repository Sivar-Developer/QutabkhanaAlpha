using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using WebAPI.Configurations;
using WebAPI.Models;
using WebAPI.Models.DTOs.Requests;
using WebAPI.Models.DTOs.Responses;

namespace WebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class OAuthController : ControllerBase
    {
        private readonly UserManager<User> _userManager;
        private readonly JwtConfig _jwtConfig;

        public OAuthController(UserManager<User> userManager, IOptionsMonitor<JwtConfig> optionsMonitor)
        {
            _userManager = userManager;
            _jwtConfig = optionsMonitor.CurrentValue;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] UserRegisterationDto user)
        {
            if (!ModelState.IsValid) return BadRequest(new RegisterationResponse()
            {
                Errors = new List<string>()
                {
                    "Invalid Payload"
                },
                Success = false
            });

            var existingUser = await _userManager.FindByEmailAsync(user.Email);

            if(existingUser != null)
            {
                return BadRequest(new RegisterationResponse()
                {
                    Errors = new List<string>()
                    {
                        "Email already taken."
                    },
                    Success = false
                });
            }

            var newUser = new User()
            {
                Email = user.Email,
                UserName = user.Username
            };

            var isCreated = await _userManager.CreateAsync(newUser, user.Password);
            if (isCreated.Succeeded)
            {
                var jwtToken = this.GenerateJwtToken(newUser);
                return Ok(new RegisterationResponse()
                {
                    Success = true,
                    Token = jwtToken
                });
            }
            else
            {
                return BadRequest(new RegisterationResponse()
                {
                    Errors = isCreated.Errors.Select(x => x.Description).ToList(),
                    Success = false
                });
            }
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] UserLoginRequest user)
        {
            if (!ModelState.IsValid) return BadRequest(new RegisterationResponse()
            {
                Errors = new List<string>()
                {
                    "Invalid Payload"
                },
                Success = false
            });

            var existingUser = await _userManager.FindByEmailAsync(user.Email);
            if(existingUser == null)
            {
                return BadRequest(new RegisterationResponse()
                {
                    Errors = new List<string>()
                    {
                        "Invalid login request."
                    },
                    Success = false
                });
            }

            var isCorrect = await _userManager.CheckPasswordAsync(existingUser, user.Password);

            if (!isCorrect)
            {
                return BadRequest(new RegisterationResponse()
                {
                    Errors = new List<string>()
                    {
                        "Invalid login request."
                    },
                    Success = false
                });
            }

            var jwtToken = this.GenerateJwtToken(existingUser);

            return Ok(new RegisterationResponse()
            {
                Success = true,
                Token = jwtToken
            });
        }

        private string GenerateJwtToken(User user)
        {
            var jwtSecurityTokenHandler = new JwtSecurityTokenHandler();

            var key = Encoding.ASCII.GetBytes(_jwtConfig.Secret);

            var securityTokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new[] {
                    new Claim("Id", user.Id),
                    new Claim(JwtRegisteredClaimNames.Email, user.Email),
                    new Claim(JwtRegisteredClaimNames.Sub, user.Email),
                    new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                }),
                Expires = DateTime.UtcNow.AddHours(6),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };

            var token = jwtSecurityTokenHandler.CreateToken(securityTokenDescriptor);
            var jwtToken = jwtSecurityTokenHandler.WriteToken(token);

            return jwtToken;
        }
    }
}
