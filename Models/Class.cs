namespace ATTENDANCE_BE.Models
{
    public class Class
    {
        public int Id { get; init; }
        public string Code { get; init; } = null!;
        public DateTime Time { get; set; } = DateTime.Now;
        public int TeacherId { get; set; }
        public string Name { get; set; } = null!;
        public string? Section { get; set; }
        public string? Subject { get; set; }
        public string? Room { get; set; }
        public bool IsArchived { get; set; } = false; 
    }

    public class ClassMember
    {
        public int ClassId { get; set; }
        public int UserId { get; set; }

        public Class Class { get; init; } = null!;
        public User User { get; init; } = null!;
    }


    public class ClassResponseDTO
    {
        public int Id { get; private set; }
        public string Code { get; init; } = null!;
        public DateTime Time { get; set; } = DateTime.Now;
        public string Name { get; set; } = null!;
        public string? Section { get; set; }
        public string? Subject { get; set; }
        public string? Room { get; set; }

        public SimpleUser Teacher { get; set; } = null!;
        public ICollection<Notification> Notifications { get; set; } = [];
        public ICollection<SimpleUser> Members { get; set; } = [];
        
        public ClassResponseDTO() {}
        public ClassResponseDTO(Class classModel)
        {
            Id = classModel.Id;
            Code = classModel.Code;
            Time = classModel.Time;
            Name = classModel.Name;
            Section = classModel.Section;
            Subject = classModel.Subject;
            Room = classModel.Room;
            Teacher = new SimpleUser { Id = classModel.TeacherId };
        }
    }

    public class ClassRequestDTO
    {
        public string Name { get; set; } = null!;
        public string? Section { get; set; }
        public string? Subject { get; set; }
        public string? Room { get; set; }

        public Class MapToClass(int tId) => new Class
        {
            Name = this.Name,
            Section = this.Section,
            Subject = this.Subject,
            Room = this.Room,
            TeacherId = tId
        };
    }
}