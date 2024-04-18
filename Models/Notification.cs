using System.ComponentModel.DataAnnotations.Schema;

namespace ATTENDANCE_BE.Models
{
    public class Notification
    {
        public int Id { get; init; }
        public DateTime Time { get; set; } = DateTime.Now;
        public int ClassId { get; set; }
        public int UserId { get; set; }
        [NotMapped]
        public string UserName { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;

    }
}