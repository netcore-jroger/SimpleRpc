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
// gRPC Client side
var tokenSource = new CancellationTokenSource(1000 * 60 * 2);
var rpcChannel = provider.GetService<IRpcChannel>();
var call = rpcChannel.AsyncClientStreamingCall<UserDto, UserDto>("greet.Greeter", "TestClientStreaming", tokenSource.Token);
await call.RequestStream.WriteAsync(new UserDto { Id = 1, Name = "abc1" });
await call.RequestStream.WriteAsync(new UserDto { Id = 2, Name = "abc2" });
await call.RequestStream.CompleteAsync();
var userDto = await call;

Console.WriteLine($"Id: {userDto.Id}, Name: {userDto.Name}, CreateDate: {userDto.CreateDate:yyyy-MM-dd HH:mm:ss fff}");
```

// gRPC Server side
- see `IUserService.cs` file line: 17-18
- see `UserService.cs` file line: 35-50

#### ServerStreaming
```csharp
// gRPC Client side
var tokenSource = new CancellationTokenSource(1000 * 60 * 2);
var rpcChannel = provider.GetService<IRpcChannel>();
var call = rpcChannel.AsyncServerStreamingCall<UserRequest, UserDto>("greet.Greeter", "TestServerStreaming", new UserRequest { Id = 1, Keyword = $"client[ServerStreaming]1: {input}" }, tokenSource.Token);
await call.ResponseStream.MoveNext(tokenSource.Token).ConfigureAwait(false);
var userDto = call.ResponseStream.Current;
Console.WriteLine($"ServerStreaming: Id: {userDto.Id}, Name: {userDto.Name}, CreateDate: {userDto.CreateDate:yyyy-MM-dd HH:mm:ss fff}");

await call.ResponseStream.MoveNext(tokenSource.Token).ConfigureAwait(false);
userDto = call.ResponseStream.Current;
Console.WriteLine($"ServerStreaming: Id: {userDto.Id}, Name: {userDto.Name}, CreateDate: {userDto.CreateDate:yyyy-MM-dd HH:mm:ss fff}");
```

// gRPC Server side
- see `IUserService.cs` file line: 20-21
- see `UserService.cs` file line: 52-78

#### DuplexStreaming
> not supported

## Roadmap

- [x] Unary supported.

- [x] ClientStreaming supported.

- [x] ServerStreaming supported.

- [ ] DuplexStreaming supported.
