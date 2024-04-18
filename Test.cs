using ATTENDANCE_BE.Data;
using ATTENDANCE_BE.Models;
using Microsoft.EntityFrameworkCore;

namespace ATTENDANCE_BE;

public class Test
{
    public Test() {
        string connectionString = "server=localhost;port=3306;database=class_attendance;user=root;password=";
        using (var context = new MyDbContext(new DbContextOptionsBuilder()
            .UseMySql(connectionString, ServerVersion.AutoDetect(connectionString))
            .Options))
        {
            Console.WriteLine(context.Database);
            
            // var person = new User()
            // {
            //     Name = "ANT2",
            //     UID = "aaaaaaaaaaaaaaaaaaaaaaaaaaaa"
            // };
            // context.Users.Add(person);
            // context.SaveChanges();

            // Your data access logic using the context instance
            
            var classes = context.Classes.ToList();
            foreach (var i in classes) {
                Console.WriteLine("Name: " + i.Name);
                // Console.WriteLine("Members:");
                // foreach (var item in i.Members)
                //     Console.WriteLine(item.Name);
            }  
            //...
        }


        Console.WriteLine("End test.");
    }
}
