using ATTENDANCE_BE.Models;
using Microsoft.EntityFrameworkCore;

namespace ATTENDANCE_BE.Data;


public partial class MyDbContext
{
    private void AttendanceBuddy(ModelBuilder modelBuilder)
    {
        var entity = modelBuilder.Entity<Attendance>();
        
        entity.HasMany(e => e.AttendanceRecords)
            .WithOne()
            .HasForeignKey(e => e.AttendanceId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<AttendanceCode>().HasKey(e => e.AttendanceId);
        modelBuilder.Entity<AttendanceRecord>().HasKey(e => new { e.AttendanceId, e.UserId });
    }
}

