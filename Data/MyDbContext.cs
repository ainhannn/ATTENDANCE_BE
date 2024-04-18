using ATTENDANCE_BE.Models;
using Microsoft.EntityFrameworkCore;

namespace ATTENDANCE_BE.Data;

public partial class MyDbContext : DbContext
{
    public MyDbContext(DbContextOptions options) : base(options) {}

    public DbSet<User> Users { get; set; }
    public DbSet<Class> Classes { get; set; }
    public DbSet<ClassMember> ClassMembers { get; set; }
    public DbSet<Notification> Notifications { get; set; }
    public DbSet<Attendance> Attendances { get; set; }
    public DbSet<AttendanceCode> AttendanceCodes { get; set; }
    public DbSet<AttendanceRecord> AttendanceRecords { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        UserBuddy(modelBuilder);
        ClassBuddy(modelBuilder);
        AttendanceBuddy(modelBuilder);
    }

}