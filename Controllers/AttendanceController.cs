using ATTENDANCE_BE.Data;
using ATTENDANCE_BE.Models;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ATTENDANCE_BE.Controllers;

[ApiController]
[EnableCors]
[Route("api/attendance")]
public class AttendanceController : ControllerBase
{
    private readonly MyDbContext _context;
    
    public AttendanceController(MyDbContext context) => _context = context;


// -- TEACHER ROLE --
    [HttpPost("create")]
    public async Task<ActionResult<Attendance>> CreateAttendance(string UID, AttendanceCreateRequestDTO attendanceRequestDTO)
    {
        var id = await _context.Users.Where(u => u.UID == UID).Select(u => u.Id).SingleOrDefaultAsync();
        if (!(id > 0)) 
            return NotFound();
        if (!await _context.Classes.AnyAsync(c => c.Id == attendanceRequestDTO.ClassId && c.TeacherId == id))
            return BadRequest();

        var attendanceModel = attendanceRequestDTO.MapToAttendance();
        _context.Attendances.Add(attendanceModel);
        await _context.SaveChangesAsync();
        
        var code = attendanceRequestDTO.MapToCode(attendanceModel.Id);
        _context.AttendanceCodes.Add(code);
        await _context.SaveChangesAsync();
        
        attendanceModel.Code = await _context.AttendanceCodes
            .Where(ac => ac.AttendanceId == attendanceModel.Id)
            .Select(ac => ac.Code)
            .SingleOrDefaultAsync();
        
        return attendanceModel;
    }

    [HttpGet("{classId}/teacher")]
    public async Task<ActionResult<IEnumerable<Attendance>>> GetAttendancesRoleTeacher(string UID, int classId)
    {
        var id = await _context.Users.Where(u => u.UID == UID).Select(u => u.Id).SingleOrDefaultAsync();
        if (!(id > 0)) 
            return NotFound();
        if (!await _context.Classes.AnyAsync(c => c.Id == classId && c.TeacherId == id))
            return BadRequest();

        var rs = await _context.Attendances
            .Where(c => c.ClassId == classId)
            .OrderByDescending(c => c.Time)
            .Join(_context.AttendanceCodes,
                a => a.Id,
                ac => ac.AttendanceId,
                (a, ac) => new Attendance {
                    Id = a.Id,
                    Time = a.Time,
                    ClassId = a.ClassId,
                    Times = a.Times,
                    Code = ac.Code
                })
            .ToListAsync();
            
        for (int i = 0; i < rs.Count; i++) {
            var aId = rs[i].Id;
            rs[i].AttendanceRecords = await _context.AttendanceRecords
                .Where(c => c.AttendanceId == aId)
                .Join(_context.Users,
                    ar => ar.UserId,
                    u => u.Id,
                    (ar, u) => new AttendanceRecord {
                        AttendanceId = ar.AttendanceId,
                        UserId = ar.UserId,
                        Time = ar.Time,
                        Status = ar.Status,
                        UserName = u.Name
                    })
                .ToListAsync();
        };

        return rs;                                                          
    }

// -- STUDENT ROLE --
    [HttpPost("take")]
    public async Task<ActionResult<Attendance>> TakeAttendance(string UID, AttendanceTakeRequestDTO dto)
    {
        // Find user and code
        var userId = await _context.Users.Where(u => u.UID == UID).Select(u => u.Id).SingleOrDefaultAsync();
        var codeModel = await _context.AttendanceCodes.Where(a => a.Code == dto.Code).SingleOrDefaultAsync();
        if (!(userId > 0 && codeModel != null)) 
            return NotFound();

        // Check user is a member of class
        if (!await _context.Attendances
            .Where(a => a.Id == codeModel.AttendanceId)
            .Join(
                _context.ClassMembers,
                a => a.ClassId,
                cm => cm.ClassId,
                (a, cm) => cm.UserId)
            .AnyAsync(u => u == userId)) 
        {
            return BadRequest("Bạn không phải thành viên lớp này!");
        }    

        // Check code time
        if (dto.Time > codeModel.ExpiryTime) 
            return BadRequest("Hết hạn điểm danh!");
        
        // Check location
        if (!Location.ApproximatelyCompare(dto.Location,codeModel.Location))
            return BadRequest("Vị trí không hợp lệ!");

        // Add
        var a = new AttendanceRecord 
        { 
            AttendanceId = codeModel.AttendanceId, 
            UserId = userId, 
            Time = dto.Time,
            Status = dto.Time > codeModel.LateTime ? AttendanceRecord.LATE : AttendanceRecord.ON_TIME 
        };
        _context.AttendanceRecords.Add(a);
        await _context.SaveChangesAsync();
        
        var rs = await _context.Attendances
            .Where(c => c.Id == a.AttendanceId)
            .OrderByDescending(c => c.Time)
            .SingleOrDefaultAsync();
        
        if (rs != null) {
            var ar = rs.AttendanceRecords;
            ar.Add(a);
            rs.AttendanceRecords = ar;
        }

        // Return
        return Ok(rs);
    }

    [HttpGet("{classId}/student")]
    public async Task<ActionResult<IEnumerable<Attendance>>> GetAttendancesRoleStudent(string UID, int classId)
    {
        var id = await _context.Users.Where(u => u.UID == UID).Select(u => u.Id).SingleOrDefaultAsync();
        if (!(id > 0 && await _context.ClassMembers.AnyAsync(c => c.ClassId == classId && c.UserId == id))) 
            return NotFound();
            
        var rs = await _context.Attendances
            .Where(c => c.ClassId == classId)
            .OrderByDescending(c => c.Time)
            .ToListAsync();
            
        for (int i = 0; i < rs.Count; i++) {
            var aId = rs[i].Id;
            rs[i].AttendanceRecords = await _context.AttendanceRecords
                .Where(ar => ar.AttendanceId == aId && ar.UserId == id)
                .Join(_context.Users,
                    ar => ar.UserId,
                    u => u.Id,
                    (ar, u) => new AttendanceRecord {
                        AttendanceId = ar.AttendanceId,
                        UserId = ar.UserId,
                        Time = ar.Time,
                        Status = ar.Status,
                        UserName = u.Name
                    })
                .ToListAsync();
        };

        return rs;
    }
}
