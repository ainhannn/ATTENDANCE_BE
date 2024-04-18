namespace ATTENDANCE_BE.Models
{
    public class User
    {
        public int Id { get; private set; }
        public DateTime Time { get; set; } = DateTime.Now;
        public string UID { get; set; } = null!;
        public string Name { get; set; } = null!;

        public ICollection<Class> CreatedClasses { get; set; } = [];
        public ICollection<Class> JoinedClasses { get; set; } = [];
    }

    public class SimpleUser 
    {
        public int Id { get; init; }
        public string? Name { get; set; }
    }

    public class UserRequestDTO 
    {
        public string Name { get; set; } = null!;
        public string UID { get; set; } = null!;

        public User MapToUser() => new User
        {
            Name = this.Name,
            UID = this.UID
        };
    }

}