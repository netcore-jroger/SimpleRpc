// Copyright (c) JRoger. All Rights Reserved.

using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;

namespace ServerSample;

public class Program
{
    public static void Main(string[] args)
    {
        CreateWebHostBuilder(args).Build().Run();
    }

    public static IWebHostBuilder CreateWebHostBuilder(string[] args) =>
        WebHost.CreateDefaultBuilder(args)
            .UseStartup<Startup>();
}
