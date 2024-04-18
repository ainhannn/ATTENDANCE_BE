using ATTENDANCE_BE.Data;
using ATTENDANCE_BE.Models;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ATTENDANCE_BE.Controllers;

[ApiController]
[EnableCors]
[Route("api/notification")]
public class NotificationController : ControllerBase
{
    private readonly MyDbContext _context;

    public NotificationController(MyDbContext context) => _context = context;

    [HttpGet("{UID}")]
    public async Task<ActionResult<IEnumerable<Notification>>> GetNotificationsByUID (string UID)
    {
        var id = await _context.Users.Where(u => u.UID == UID).Select(u => u.Id).SingleOrDefaultAsync();
        if (!(id > 0)) return NotFound();

        var notifications = await _context.Notifications
            .Where(n => 
                _context.ClassMembers
                    .Where(cm => cm.UserId == id)
                    .Select(cm => cm.ClassId)
                    .Contains(n.ClassId) 
                ||
                _context.Classes
                    .Where(c => c.TeacherId == id)
                    .Select(c => c.Id)
                    .Contains(n.ClassId))
            .Join(
                _context.Users,
                n => n.UserId,
                u => u.Id,
                (n, u) => new Notification {
                    Id = n.Id,
                    Time = n.Time,
                    ClassId = n.ClassId,
                    UserId = n.UserId,
                    UserName = u.Name,
                    Content = n.Content
                })
            .ToListAsync();

        return Ok(notifications);
    }
}
