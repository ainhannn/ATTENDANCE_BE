using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;

namespace ATTENDANCE_BE;

public class Startup
{
    private IConfiguration? Configuration { get; }
    
    public void ConfigureServices(IServiceCollection services)
    {
        services.AddControllers();
        
        // Add  API services
        services.AddEndpointsApiExplorer();
        services.AddSwaggerGen(c =>
            c.SwaggerDoc("v1", new OpenApiInfo { Title = "Attendance API", Description = "Class Attendance BE API", Version = "v1" }));
        services.AddDbContext<Data.MyDbContext>(options => 
        {
            string? connectionString = Configuration?.GetConnectionString("MySqlConnection");
            connectionString ??= "server=localhost;port=3306;database=class_attendance;user=root;password=";
            options.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString));
        });
    }

    public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
    {
        if (env.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI(c =>
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "Attendance API V1"));
        }
        
        app.UseHttpsRedirection();
        app.UseRouting();
        app.UseCors();
        app.UseEndpoints(endpoints =>
        {
            endpoints.MapControllers();
            endpoints.MapGet("/", () => "Hello World!");
        });
    }
}