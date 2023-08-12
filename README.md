# SimpleRpc
A light-weight RPC wrap of google gRPC framework.

![Build Status](https://github.com/netcore-jroger/SimpleRpc/workflows/SimpleRpc-CI/badge.svg)
![GitHub](https://img.shields.io/github/license/netcore-jroger/SimpleRpc.svg)

| **Package** | **NuGet** |
| --- | --- |
| SimpleRpc.Shared | ![Nuget](https://img.shields.io/nuget/v/SimpleRpc.Shared.svg) |
| SimpleRpc.Server | ![Nuget](https://img.shields.io/nuget/v/SimpleRpc.Server.svg) |
| SimpleRpc.Client | ![Nuget](https://img.shields.io/nuget/v/SimpleRpc.Client.svg) |

## Getting Started

```csharp
// gRPC client side
var configuration = new ConfigurationBuilder()
    .AddJsonFile("appsettings.json", false, true)
    .Build();
var provider = new ServiceCollection()
    .AddRpcClient(configuration)
    .AddRpcClientService<IUserService>()
    .BuildServiceProvider();
```

#### Unary
```csharp
// gRPC client side
var tokenSource = new CancellationTokenSource(1000 * 60 * 2);
var userService = provider.GetService<IUserService>();
var userDto = await userService.GetUserBy(userRequest, tokenSource.Token);

```

#### ClientStreaming
```csharp
// gRPC client side
var tokenSource = new CancellationTokenSource(1000 * 60 * 2);
var rpcChannel = provider.GetService<IRpcChannel>();
var call = rpcChannel.AsyncClientStreamingCall<UserDto, UserDto>("greet.Greeter", "TestClientStreaming", tokenSource.Token);
await call.RequestStream.WriteAsync(new UserDto { Id = 1, Name = "abc1" });
await call.RequestStream.WriteAsync(new UserDto { Id = 2, Name = "abc2" });
await call.RequestStream.CompleteAsync();
var userDto = await call;

Console.WriteLine($"Id: {userDto.Id}, Name: {userDto.Name}, CreateDate: {userDto.CreateDate:yyyy-MM-dd HH:mm:ss fff}");
```

#### ServerStreaming
> not supported

#### DuplexStreaming
> not supported

## Roadmap

- [x] Unary supported.

- [x] ClientStreaming supported.

- [ ] ServerStreaming supported.

- [ ] DuplexStreaming supported.
