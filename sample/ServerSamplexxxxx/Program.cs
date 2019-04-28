using InterfaceLib;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SimpleRpc.Server;
using SimpleRpc.Shared;
using System;
using System.Collections.Generic;

namespace ServerSample
{
    class Program
    {
        static void Main(string[] args)
        {
            var configuration = new ConfigurationBuilder()
                .AddJsonFile("appsetting.json", false, true).Build();
            var services = new ServiceCollection()
                .AddTransient<IUserService, UserService>()
                .AddSingleton<IRpcServiceTypeFinder, FakeRpcServiceTypeFinder>()
                .AddRpcServer(options => {
                    configuration.GetSection(RpcConfigInformation.RpcServerConfigSectionName).Bind(options);
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
    }
}
