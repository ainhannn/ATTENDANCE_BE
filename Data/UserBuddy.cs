using ATTENDANCE_BE.Models;
using Microsoft.EntityFrameworkCore;

namespace ATTENDANCE_BE.Data;


public partial class MyDbContext
{
    private void UserBuddy(ModelBuilder modelBuilder)
    {
        var entity = modelBuilder.Entity<User>();
        
        entity.Property(p => p.UID)
            .HasMaxLength(28)
            .IsFixedLength()
            .IsRequired();
        entity.Property(p => p.Name)
            .HasMaxLength(30)
            .IsRequired();
        
        entity.HasMany(e => e.CreatedClasses)
            .WithOne()
            .HasForeignKey(e => e.TeacherId)
            .OnDelete(DeleteBehavior.SetNull)
            .IsRequired();

        entity.HasMany(e => e.JoinedClasses)
            .WithMany()
            .UsingEntity<ClassMember>(
                l => l.HasOne(e => e.Class).WithMany().HasForeignKey(e => e.ClassId),
                r => r.HasOne(e => e.User).WithMany().HasForeignKey(e => e.UserId)
            );
    }

}

