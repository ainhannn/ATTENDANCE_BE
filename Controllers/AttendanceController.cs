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
            .Join(
                _context.AttendanceCodes,
                a => a.Id,
                ac => ac.AttendanceId,
                (a, ac) => new { Attendance = a, ac.Code }) // Call x, select AttendanceCode here
            .Join(
                _context.AttendanceRecords.OrderBy(c => c.Time),
                x => x.Attendance.Id,
                ar => ar.AttendanceId,
                (x, ar) => new { x.Attendance, x.Code, AttendanceRecord = ar}) // Call y, include records here
            .Join(
                _context.Users,
                y => y.AttendanceRecord.UserId,
                u => u.Id,
                (y, u) => new { y.Attendance, y.Code, y.AttendanceRecord, u.Name}) // Call joinedData, select User.Name here
            
            .Select(joinedData => new Attendance {
                Id = joinedData.Attendance.Id,
                Time = joinedData.Attendance.Time,
                ClassId = joinedData.Attendance.ClassId,
                Times = joinedData.Attendance.Times,
                Code = joinedData.Code,
                AttendanceRecords = new List<AttendanceRecord>
                {
                    new AttendanceRecord
                    {
                        AttendanceId = joinedData.AttendanceRecord.AttendanceId,
                        UserId = joinedData.AttendanceRecord.UserId,
                        Time = joinedData.AttendanceRecord.Time,
                        Status = joinedData.AttendanceRecord.Status,
                        UserName = joinedData.Name
                    }
                }
            })
            .ToListAsync();

            return rs;
    }

// -- STUDENT ROLE --
    [HttpPost("take")]
    public async Task<ActionResult<AttendanceRecord>> TakeAttendance(string UID, AttendanceTakeRequestDTO dto)
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
        var rs = new AttendanceRecord 
        { 
            AttendanceId = codeModel.AttendanceId, 
            UserId = userId, 
            Time = dto.Time,
            Status = dto.Time > codeModel.LateTime ? AttendanceRecord.LATE : AttendanceRecord.ON_TIME 
        };
        _context.AttendanceRecords.Add(rs);
        await _context.SaveChangesAsync();
        
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
            .Join(
                _context.AttendanceRecords.Where(ar => ar.UserId == id),
                a => a.Id,
                ar => ar.AttendanceId,
                (a, ar) => new { Attendance = a, AttendanceRecord = ar}) // Call y, include records here
            .Join(
                _context.Users,
                y => y.AttendanceRecord.UserId,
                u => u.Id,
                (y, u) => new { y.Attendance, y.AttendanceRecord, u.Name}) // Call joinedData, select User.Name here
            
            .Select(joinedData => new Attendance {
                Id = joinedData.Attendance.Id,
                Time = joinedData.Attendance.Time,
                ClassId = joinedData.Attendance.ClassId,
                Times = joinedData.Attendance.Times,
                AttendanceRecords = new List<AttendanceRecord>
                {
                    new AttendanceRecord
                    {
                        AttendanceId = joinedData.AttendanceRecord.AttendanceId,
                        UserId = joinedData.AttendanceRecord.UserId,
                        Time = joinedData.AttendanceRecord.Time,
                        Status = joinedData.AttendanceRecord.Status,
                        UserName = joinedData.Name
                    }
                }
            })
            .ToListAsync();

        return rs;
    }
}
