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

    [HttpGet("{UID}/all")]
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
            .OrderByDescending(c => c.Time)
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

    [HttpGet("{classId}")]
    public async Task<ActionResult<IEnumerable<Notification>>> GetNotificationsByClassId (int classId)
    {
        if (!await _context.Classes.AnyAsync(c => c.Id == classId))
            return NotFound();

        var notifications = await _context.Notifications
            .Where(n => n.ClassId == classId)
            .OrderByDescending(c => c.Time)
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

    [HttpPost("create")]
    public async Task<ActionResult<Notification>> CreateNotification(int classId, string UID, string content)
    {
        // Check user exist
        var id = await _context.Users.Where(u => u.UID == UID).Select(u => u.Id).SingleOrDefaultAsync();
        if (!(id > 0)) 
            return NotFound();

        // Check user in class (be a teacher or a student)
        if (!await _context.Classes.AnyAsync(c => c.Id == classId && 
                (c.TeacherId == id || 
                _context.ClassMembers.Where(cm => cm.UserId == id).Select(cm => cm.ClassId).Contains(c.Id))))
            return BadRequest();

        var notification = new Notification {
            ClassId = classId,
            UserId = id,
            Content = content
        };
        
        _context.Notifications.Add(notification);
        await _context.SaveChangesAsync();
        
        notification.UserName = await _context.Users.Where(u => u.UID == UID).Select(u => u.Name).SingleOrDefaultAsync() ?? string.Empty;
        return notification;
    }
}
