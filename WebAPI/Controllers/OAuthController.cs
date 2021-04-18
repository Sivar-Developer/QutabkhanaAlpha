using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
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
using WebAPI.Data;
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
        private readonly TokenValidationParameters _tokenValidationParameters;
        private readonly ApplicationDbContext _context;

        public OAuthController(UserManager<User> userManager, IOptionsMonitor<JwtConfig> optionsMonitor, TokenValidationParameters tokenValidationParameters, ApplicationDbContext context)
        {
            _userManager = userManager;
            _jwtConfig = optionsMonitor.CurrentValue;
            _tokenValidationParameters = tokenValidationParameters;
            _context = context;
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
                var jwtToken = await this.GenerateJwtToken(newUser);
                return Ok(jwtToken);
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

            var jwtToken = await this.GenerateJwtToken(existingUser);

            return Ok(jwtToken);
        }

        [HttpPost("RefreshToken")]
        public async Task<IActionResult> RefreshToken([FromBody] TokenRequest tokenRequest)
        {
            if (!ModelState.IsValid) return BadRequest(new RegisterationResponse()
            {
                Errors = new List<string>()
                {
                    "Invalid payload"
                },
                Success = false
            });

            var result = await VerifyAndGenerateToken(tokenRequest);
            if (result == null) return BadRequest(new RegisterationResponse()
            {
                Errors = new List<string>()
                {
                    "Invalid tokens"
                },
                Success = false
            });

            return Ok(result);
        }

        private async Task<AuthResult> GenerateJwtToken(User user)
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

            var refreshToken = new RefreshToken()
            {
                JwtId = token.Id,
                IsUsed = false,
                IsRevoked = false,
                UserId = user.Id,
                AddedDate = DateTime.UtcNow,
                ExpiryDate = DateTime.UtcNow.AddMonths(6),
                Token = RandomString(35) + Guid.NewGuid()
            };

            await _context.RefreshTokens.AddAsync(refreshToken);
            await _context.SaveChangesAsync();

            return new AuthResult() {
                Token = jwtToken,
                Success = true,
                RefreshToken = refreshToken.Token
            };
        }

        private async Task<AuthResult> VerifyAndGenerateToken(TokenRequest tokenRequest)
        {
            var jwtTokenHandler = new JwtSecurityTokenHandler();

            try
            {
                // Validation 1
                var tokenInVerification = jwtTokenHandler.ValidateToken(tokenRequest.Token, _tokenValidationParameters, out var validatedToken);

                // Validation 2 - Validate encryption alg
                if(validatedToken is JwtSecurityToken jwtSecurityToken)
                {
                    var result = jwtSecurityToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha256, StringComparison.InvariantCultureIgnoreCase);

                    if(result == false)
                    {
                        return null;
                    }
                }

                // Validation 3 - Validate expiry data
                var utcExpiryDate = long.Parse(tokenInVerification.Claims.FirstOrDefault(x => x.Type == JwtRegisteredClaimNames.Exp).Value);

                var expiryDate = UnixTimeStampToDateTime(utcExpiryDate);

                // Bug
                if(expiryDate > DateTime.UtcNow)
                {
                    return new AuthResult()
                    {
                        Success = false,
                        Errors = new List<string>()
                        {
                            "Token has not yet expired"
                        }
                    };
                }

                // Validation 4 - validate existence of the token
                var storedToken = await _context.RefreshTokens.FirstOrDefaultAsync(x => x.Token == tokenRequest.RefreshToken);
                if(storedToken == null)
                {
                    return new AuthResult()
                    {
                        Success = false,
                        Errors = new List<string>()
                        {
                            "Token does not exist"
                        }
                    };
                }

                // Validation 5
                if (storedToken.IsUsed)
                {
                    return new AuthResult()
                    {
                        Success = false,
                        Errors = new List<string>()
                        {
                            "Token has been used"
                        }
                    };
                }

                // Validation 6 - validate if revoked
                if(storedToken.IsRevoked)
                {
                    return new AuthResult()
                    {
                        Success = false,
                        Errors = new List<string>()
                        {
                            "Token has been revoked"
                        }
                    };
                }

                // Validation 7 - validate the id
                var jti = tokenInVerification.Claims.FirstOrDefault(x => x.Type == JwtRegisteredClaimNames.Jti).Value;

                if(storedToken.JwtId != jti)
                {
                    return new AuthResult()
                    {
                        Success = false,
                        Errors = new List<string>()
                        {
                            "Token does not match"
                        }
                    };
                }

                // update current token
                storedToken.IsUsed = true;
                _context.RefreshTokens.Update(storedToken);
                await _context.SaveChangesAsync();

                // Generate new token
                var dbUser = await _userManager.FindByIdAsync(storedToken.UserId);
                return await GenerateJwtToken(dbUser);
            }
            catch(Exception ex)
            {
                return null;
            }
        }

        private DateTime UnixTimeStampToDateTime(long unixTimeStamp)
        {
            var dateTimeVal = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
            dateTimeVal = dateTimeVal.AddSeconds(unixTimeStamp).ToLocalTime();
            return dateTimeVal;
        }

        private static string RandomString(int length)
        {
            var random = new Random();
            var chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            return new string(Enumerable.Repeat(chars, length).Select(x => x[random.Next(x.Length)]).ToArray());
        }
    }
}
