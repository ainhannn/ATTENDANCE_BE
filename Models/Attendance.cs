using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore.Metadata.Conventions;

namespace ATTENDANCE_BE.Models {
    public class Attendance
    {
        public int Id { get; set; }
        public DateTime Time { get; set; } = DateTime.Now;
        public int ClassId { get; set; }
        public int Times { get; set; } 
        
        public ICollection<AttendanceRecord> AttendanceRecords { get; set; } = [];
        
        [NotMapped]
        public string? Code { get; set; }
        [NotMapped]
        public int PresentCount { 
            get {
                int c = 0;
                foreach (var record in AttendanceRecords) 
                    if (record.Status == AttendanceRecord.ON_TIME) 
                        c++;
                return c;
            }
        }
        [NotMapped]
        public int LateCount { 
            get {
                int c = 0;
                foreach (var record in AttendanceRecords) 
                    if (record.Status == AttendanceRecord.LATE) 
                        c++;
                return c;
            }
        }
    }

    public class AttendanceCode
    {
        public int AttendanceId { get; set; }
        public string? Code { get; set; }
        public string Location { get; set; } = null!;
        public DateTime LateTime { get; set; }
        public DateTime ExpiryTime { get; set; }
    }

    public class AttendanceRecord
    {
        public static readonly int LATE = 0;
        public static readonly int ON_TIME = 1;

        public int AttendanceId { get; set; }
        public int UserId { get; set; }
        public DateTime Time { get; set; } = DateTime.Now;
        public int Status { get; set; }

        [NotMapped]
        public string UserName { get; set; } = string.Empty;
    }

    public class AttendanceCreateRequestDTO {
        public int ClassId { get; set; }
        public string Location { get; set; } = null!;
        public DateTime CreatedTime { get; set; } = DateTime.Now;
        public int LateAfter { get; set; }
        public int ExpiryAfter { get; set; }
    
        public Attendance MapToAttendance() => new Attendance
        {
            Time = CreatedTime,
            ClassId = this.ClassId
        };
        public AttendanceCode MapToCode(int aId) => new AttendanceCode
        { 
            AttendanceId = aId,
            Location = this.Location,
            LateTime = CreatedTime.AddMinutes(LateAfter),
            ExpiryTime = CreatedTime.AddMinutes(ExpiryAfter)
        };
    }

    public class AttendanceTakeRequestDTO {
        public string Location { get; set; } = null!;
        public DateTime Time { get; set; } = DateTime.Now;
        public string Code { get; set; } = null!;
    }
}