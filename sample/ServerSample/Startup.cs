using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using InterfaceLib;
using SimpleRpc.Server;
using SimpleRpc.Shared;
using SimpleRpc.Shared.Description;

namespace ServerSample
{
    public class Startup
    {
        public void ConfigureServices(IServiceCollection services)
        {
            var configuration = new ConfigurationBuilder().AddJsonFile("appsettings.json", false, true).Build();

            //services.AddSingleton<IRpcServiceTypeFinder, FakeRpcServiceTypeFinder>();
            services.AddRpcServer(option => {
                configuration.GetSection(RpcConfigInformation.RpcServerConfigSectionName).Bind(option);
            });
            services.AddScoped<IUserService, UserService>();
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.Run(async (context) =>
            {
                await context.Response.WriteAsync("Hello World!");
            });
        }
    }

    public class FakeRpcServiceTypeFinder : IRpcServiceTypeFinder
    {
        public List<Type> FindAllRpcServiceType()
        {
            return new List<Type> {
                typeof(IUserService)
            };
        }

        public List<RpcServiceDescription> GetAllRpcServiceDescription()
        {
            throw new NotImplementedException();
        }
    }
}
