﻿using InterfaceLib;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SimpleRpc.Client;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace ClientSample
{
    class Program
    {
        async static Task Main(string[] args)
        {
            var configuration = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json", false, true)
                .Build();
            var provider = new ServiceCollection()
                .AddRpcClient(configuration)
                .AddRpcClientService<IUserService>()
                .BuildServiceProvider();

            var userService = provider.GetService<IUserService>();
            var userRequest = new UserRequest {
                Id = 1
            };

            while (true)
            {
                var tokenSource = new CancellationTokenSource(1000 * 50);
                Console.Write("Please input keyword:");
                var input = Console.ReadLine();
                if (input.Equals("Q", StringComparison.OrdinalIgnoreCase))
                {
                    break;
                }

                userRequest.Keyword = input;
                var userDto = await userService.GetUserBy(userRequest, tokenSource.Token);

                Console.WriteLine($"Id: {userDto.Id}, Name: {userDto.Name}, CreateDate: {userDto.CreateDate:yyyy-MM-dd HH:mm:ss fff}");
            }

            Console.WriteLine("press any key to exit.");
            Console.ReadKey(true);
        }
    }
}
