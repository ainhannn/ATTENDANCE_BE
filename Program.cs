var HostBuilder = Host
    .CreateDefaultBuilder(args)
    .ConfigureWebHostDefaults(webBuilder => 
        webBuilder.UseStartup<ATTENDANCE_BE.Startup>());

var host = HostBuilder.Build();
host.Run();

// new ATTENDANCE_BE.Test();


