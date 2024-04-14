// Copyright (c) JRoger. All Rights Reserved.

using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SimpleRpc.Client;
using InterfaceLib;

namespace ClientSample;

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

        var userRequest = new UserRequest {
            Id = 1
        };

        while (true)
        {
            Console.Write("Please input keyword:");
            var input = Console.ReadLine();
            if (input.Equals("Q", StringComparison.OrdinalIgnoreCase))
            {
                break;
            }

            if (input.StartsWith("cs:", StringComparison.OrdinalIgnoreCase))
            {
                userRequest.Keyword = input;
                var tokenSource = new CancellationTokenSource(1000 * 60 * 2);
                var rpcChannel = provider.GetService<IRpcChannel>();
                var call = rpcChannel.AsyncClientStreamingCall<UserDto, UserDto>("greet.Greeter", "TestClientStreaming", tokenSource.Token);
                await call.RequestStream.WriteAsync(new UserDto { Id = 1, Name = "client[ClientStreaming]1" });
                await call.RequestStream.WriteAsync(new UserDto { Id = 2, Name = "client[ClientStreaming]2" });
                await call.RequestStream.CompleteAsync();
                var userDto = await call;

                Console.WriteLine($"ClientStreaming: Id: {userDto.Id}, Name: {userDto.Name}, CreateDate: {userDto.CreateDate:yyyy-MM-dd HH:mm:ss fff}");
            }
            else if (input.StartsWith(value: "ss:", StringComparison.OrdinalIgnoreCase))
            {
                var tokenSource = new CancellationTokenSource(1000 * 60 * 2);
                var rpcChannel = provider.GetService<IRpcChannel>();
                var call = rpcChannel.AsyncServerStreamingCall<UserRequest, UserDto>("greet.Greeter", "TestServerStreaming", new UserRequest { Id = 1, Keyword = $"client[ServerStreaming]1: {input}" }, tokenSource.Token);
                await call.ResponseStream.MoveNext(tokenSource.Token).ConfigureAwait(false);
                var userDto = call.ResponseStream.Current;
                Console.WriteLine($"ServerStreaming: Id: {userDto.Id}, Name: {userDto.Name}, CreateDate: {userDto.CreateDate:yyyy-MM-dd HH:mm:ss fff}");

                await call.ResponseStream.MoveNext(tokenSource.Token).ConfigureAwait(false);
                userDto = call.ResponseStream.Current;
                Console.WriteLine($"ServerStreaming: Id: {userDto.Id}, Name: {userDto.Name}, CreateDate: {userDto.CreateDate:yyyy-MM-dd HH:mm:ss fff}");
            }
            else
            {
                userRequest.Keyword = input;
                var tokenSource = new CancellationTokenSource(1000 * 60 * 2);
                var userService = provider.GetService<IUserService>();
                var userDto = await userService.GetUserBy(userRequest, tokenSource.Token);

                Console.WriteLine($"Unary: Id: {userDto.Id}, Name: {userDto.Name}, CreateDate: {userDto.CreateDate:yyyy-MM-dd HH:mm:ss fff}");
            }
        }

        Console.WriteLine("press any key to exit.");
        Console.ReadKey(true);
    }
}
