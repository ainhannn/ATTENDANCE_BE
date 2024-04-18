using ATTENDANCE_BE.Models;
using Microsoft.EntityFrameworkCore;

namespace ATTENDANCE_BE.Data;


public partial class MyDbContext
{
    public object Configuration { get; internal set; }

    private void ClassBuddy(ModelBuilder modelBuilder)
    {
        var entity = modelBuilder.Entity<Class>();
        
        entity.Property(p => p.Name)
            .HasMaxLength(255)
            .IsRequired();
        entity.Property(p => p.Section)
            .HasMaxLength(255)
            .IsRequired(false);
        entity.Property(p => p.Subject)
            .HasMaxLength(255)
            .IsRequired(false);
        entity.Property(p => p.Room)
            .HasMaxLength(255)
            .IsRequired(false);
    }

}

