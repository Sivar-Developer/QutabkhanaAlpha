using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WebAPI.Data;

namespace WebAPI.Controllers
{
    [ApiController]
    public class GeneralController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public GeneralController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet("/api/stages")]
        public async Task<IActionResult> stagesIndex()
        {
            var stages = await _context.Stages.ToListAsync();
            if (stages == null) return NotFound();

            return Ok(stages);
        }
    }
}
