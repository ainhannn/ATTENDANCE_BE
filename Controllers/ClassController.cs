using ATTENDANCE_BE.Data;
using ATTENDANCE_BE.Models;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Internal;


namespace ATTENDANCE_BE.Controllers;

[ApiController]
[EnableCors]
[Route("api/class")]
public class ClassController : ControllerBase
{
    private readonly MyDbContext _context;

    public ClassController(MyDbContext context) => _context = context;


    [HttpGet("{classId}")]
    public async Task<ActionResult<ClassResponseDTO>> GetCompleteClass(int classId)
    {
        var classModel = await _context.Classes.FindAsync(classId);
        if (classModel == null) return NotFound();
        
        var dto = new ClassResponseDTO(classModel);

        dto.Teacher.Name = await _context.Users
            .Where(c => c.Id == classModel.TeacherId)
            .Select(c => c.Name)
            .SingleOrDefaultAsync();

        dto.Notifications = await _context.Notifications
            .Where(c => c.ClassId == classModel.Id)
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
        

        dto.Members = await _context.ClassMembers
            .Where(cm => cm.ClassId == classModel.Id)
            .Join(
                _context.Users, 
                cm => cm.UserId, 
                u => u.Id, 
                (cm, u) => new SimpleUser { Id = u.Id, Name = u.Name })
            .OrderBy(c => c.Name)
            .ToListAsync();

        return Ok(dto);
    }

    [HttpPut("{classId}/update")]
    public async Task<IActionResult> UpdateClassInformation(int classId, ClassRequestDTO classRequestDTO)
    {
        try
        {
            var classExisted = await _context.Classes.FindAsync(classId);
            if (classExisted == null) return NotFound();
            
            classExisted.Name = classRequestDTO.Name;
            classExisted.Section = classRequestDTO.Section;
            classExisted.Subject = classRequestDTO.Subject;
            classExisted.Room = classRequestDTO.Room;

            await _context.SaveChangesAsync();

            return NoContent();
        }
        catch (Exception)
        {
            return BadRequest();
        }
    }

    [HttpPost("create")]
    public async Task<ActionResult<ClassResponseDTO>> CreateClass(string UID, ClassRequestDTO classRequestDTO)
    {
        var id = await _context.Users.Where(u => u.UID == UID).Select(u => u.Id).SingleOrDefaultAsync();
        if (!(id > 0)) return NotFound();

        var classModel = classRequestDTO.MapToClass(id);
        _context.Classes.Add(classModel);
        await _context.SaveChangesAsync();
        
        return await GetCompleteClass(classModel.Id);
    }

    [HttpPost("join")]
    public async Task<ActionResult<ClassResponseDTO>> JoinClass(string UID, string classCode)
    {
        var userId = await _context.Users.Where(u => u.UID == UID).Select(u => u.Id).SingleOrDefaultAsync();
        var classId = await _context.Classes.Where(u => u.Code == classCode).Select(u => u.Id).SingleOrDefaultAsync();
        if (!(classId > 0 && userId > 0)) return NotFound();

        _context.ClassMembers
            .Add(new ClassMember { ClassId = classId, UserId = userId});
        await _context.SaveChangesAsync();
        
        return await GetCompleteClass(classId);
    }

    [HttpPut("{classId}/archive")]
    public async Task<IActionResult> ArchiveClass(int classId)
    {
        try
        {
            var classExisted = await _context.Classes.FindAsync(classId);
            if (classExisted == null) return NotFound(); 
            
            classExisted.IsArchived = true;
            await _context.SaveChangesAsync();

            return NoContent();
        }
        catch (Exception)
        {
            return BadRequest();
        }
    }

    [HttpPut("{classId}/restore")]
    public async Task<IActionResult> RestoreClass(int classId)
    {
        try
        {
            var classExisted = await _context.Classes.FindAsync(classId);
            if (classExisted == null) return NotFound(); 
            
            classExisted.IsArchived = false;
            await _context.SaveChangesAsync();

            return NoContent();
        }
        catch (Exception)
        {
            return BadRequest();
        }
    }

    [HttpDelete("{classId}/delete")]
    public async Task<IActionResult> DeleteClass(int classId)
    {
        var classExisted = await _context.Classes.FindAsync(classId);
        if (classExisted == null)
        {
            return NotFound();
        }

        _context.Classes.Remove(classExisted);
        await _context.SaveChangesAsync();

        return NoContent();
    }

    [HttpDelete("{classId}/out")]
    public async Task<IActionResult> OutClass(int userId, int classId)
    {
        try {
            _context.ClassMembers.Remove(new ClassMember { ClassId = classId, UserId = userId});
            await _context.SaveChangesAsync();
            return NoContent();
        }
        catch (Exception)
        {
            return NotFound();
        }
    }
}
