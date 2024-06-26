// Copyright (c) JRoger. All Rights Reserved.

using System;
using System.Text.Json;
using System.Text.Encodings.Web;
using System.Text.Unicode;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using InterfaceLib;
using SimpleRpc.Server;

namespace ServerSample;

public class UserService : RpcServiceBase, IUserService
{
    private static readonly JsonSerializerOptions _options = new JsonSerializerOptions { Encoder = JavaScriptEncoder.Create(UnicodeRanges.All) };
    private readonly ILogger<UserService> _logger;

    public UserService(ILoggerFactory loggerFactory)
    {
        this._logger = loggerFactory.CreateLogger<UserService>();
    }

    public Task<UserDto> TestUnary(UserRequest request, CancellationToken token = default)
    {
        this._logger.LogInformation($"Receive client Unary message：{JsonSerializer.Serialize(request, _options)}");

        return Task.FromResult(new UserDto {
            Id = (int)DateTime.Now.Ticks / 10000,
            Name = Guid.NewGuid().ToString("D") + request.Keyword,
            CreateDate = DateTime.Now
        });
    }

    public async Task<UserDto> TestClientStreaming(CancellationToken token = default)
    {
        var requestStream = this.GetAsyncStreamReader<UserDto>();

        while ( await requestStream.MoveNext(token).ConfigureAwait(false) )
        {
            this._logger.LogInformation($"Receive client ClientStreaming message：{JsonSerializer.Serialize(requestStream.Current)}");
        }

        return new UserDto {
            Id = (int)DateTime.Now.Ticks / 10000,
            Name = Guid.NewGuid().ToString("D"),
            CreateDate = DateTime.Now
        };
    }

    public async Task TestServerStreaming(UserRequest request, CancellationToken token = default)
    {
        this._logger.LogInformation($"Receive client ServerStreaming message：{JsonSerializer.Serialize(request)}");

        var responseStream = this.GetServerStreamWriter<UserDto>();

        // NOTE: do not use method signature: Task WriteAsync(T message, CancellationToken cancellationToken)
        await responseStream.WriteAsync(
            new UserDto {
                Id = (int)DateTime.Now.Ticks / 10000,
                Name = Guid.NewGuid().ToString("D"),
                CreateDate = DateTime.Now
            }
        );

        await Task.Delay(1000 * 2, token);

        await responseStream.WriteAsync(
            new UserDto {
                Id = (int)DateTime.Now.Ticks / 10000,
                Name = Guid.NewGuid().ToString("D"),
                CreateDate = DateTime.Now
            }
        );
    }

    public async Task TestDuplexStreaming(CancellationToken token = default)
    {
        var responseStream = this.GetServerStreamWriter<UserDto>();

        // NOTE: do not use method signature: Task WriteAsync(T message, CancellationToken cancellationToken)
        await responseStream.WriteAsync(
            new UserDto {
                Id = (int)DateTime.Now.Ticks / 10000,
                Name = $"From Server --> DuplexStreaming: {Guid.NewGuid():D}",
                CreateDate = DateTime.Now
            }
        );

        await Task.Delay(1000 * 2, token);

        await responseStream.WriteAsync(
            new UserDto {
                Id = (int)DateTime.Now.Ticks / 10000,
                Name = $"From Server --> DuplexStreaming: {Guid.NewGuid():D}",
                CreateDate = DateTime.Now
            }
        );

        var requestStream = this.GetAsyncStreamReader<UserRequest>();

        while ( await requestStream.MoveNext(token).ConfigureAwait(false) )
        {
            this._logger.LogInformation($"Receive client DuplexStreaming message：{JsonSerializer.Serialize(requestStream.Current)}");
        }
    }
}
