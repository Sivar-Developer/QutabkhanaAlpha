using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using WebAPI.Data;
using WebAPI.Models;
using WebAPI.Models.DTOs.Requests;

namespace WebAPI.Controllers
{
    [ApiController]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public class UserController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<User> _userManager;

        public UserController(ApplicationDbContext context, UserManager<User> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        [HttpGet("api/user/profile")]
        public async Task<IActionResult> UserDetails()
        {
            var email = User.Claims.Where(x => x.Type == ClaimTypes.Email).FirstOrDefault()?.Value;
            var user = await _userManager.FindByEmailAsync(email);

            return Ok(user);
        }

        [HttpPatch("api/user/stage")]
        public async Task<IActionResult> StageChange([FromBody] StageRequest request)
        {
            var email = User.Claims.Where(x => x.Type == ClaimTypes.Email).FirstOrDefault()?.Value;
            var user = await _userManager.FindByEmailAsync(email);
            user.StageId = request.StageId;
            await _context.Users.FindAsync(user.Id);
            _context.Update(user);
            _context.SaveChanges();

            return Ok(user);
        }
    }
}
