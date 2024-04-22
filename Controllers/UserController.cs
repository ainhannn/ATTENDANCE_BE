using ATTENDANCE_BE.Data;
using ATTENDANCE_BE.Models;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ATTENDANCE_BE.Controllers
{
    [ApiController]
    [EnableCors]
    [Route("api/user")]
    public class UserController : ControllerBase
    {
        private readonly MyDbContext _context;

        public UserController(MyDbContext context) => _context = context;


        [HttpGet("login")]
        public async Task<ActionResult<User>> Login(string UID)
        {
            var user = await _context.Users
                .Include(u => u.CreatedClasses.Where(p => !p.IsArchived).OrderByDescending(c => c.Time))    
                .Include(u => u.JoinedClasses.Where(p => !p.IsArchived).OrderByDescending(c => c.Time))
                .Where(u => u.UID == UID)
                .SingleOrDefaultAsync();
            
            return (user != null) ? user : NotFound();
        }

        [HttpPost("register")]
        public async Task<ActionResult<User>> Register(UserRequestDTO userRequestDTO)
        {
            try
            {
                var user = userRequestDTO.MapToUser();
                _context.Users.Add(user);
                var rs = await _context.SaveChangesAsync();
                return user;
            }
            catch (Exception)
            {
                return BadRequest();
            }

        }

        [HttpPut("{UID}/update")]
        public async Task<IActionResult> UpdateDisplayName(string UID, string name)
        {
            try
            {
                var user = await _context.Users.SingleOrDefaultAsync(u => u.UID == UID);
                if (user == null) return NotFound(); // Handle case where entity is not found

                user.Name = name;
                await _context.SaveChangesAsync();

                return NoContent();
            }
            catch (Exception)
            {
                return BadRequest();
            }
        }
    }
}