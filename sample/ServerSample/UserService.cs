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

    public Task<UserDto> GetUserBy(UserRequest request, CancellationToken token = default)
    {
        this._logger.LogInformation($"Receive client message：{JsonSerializer.Serialize(request, _options)}");

        return Task.FromResult(new UserDto
        {
            Id = (int)DateTime.Now.Ticks / 10000,
            Name = Guid.NewGuid().ToString("D") + request.Keyword,
            CreateDate = DateTime.Now
        });
    }

    public async Task<UserDto> TestClientStreaming(CancellationToken token = default)
    {
        var requestStream = this.GetAsyncStreamReader<UserDto>();

        while (await requestStream.MoveNext(token).ConfigureAwait(false))
        {
            this._logger.LogInformation($"Receive client client stream message：{JsonSerializer.Serialize(requestStream.Current)}");
        }

        return new UserDto
        {
            Id = (int)DateTime.Now.Ticks / 10000,
            Name = Guid.NewGuid().ToString("D"),
            CreateDate = DateTime.Now
        };
    }

    public async Task TestServerStreaming(UserDto request, CancellationToken token = default)
    {
        this._logger.LogInformation($"Receive client server streaming message：{JsonSerializer.Serialize(request)}");

        var responseStream = this.GetServerStreamWriter<UserDto>();

        await responseStream.WriteAsync(
            new UserDto
            {
                Id = (int)DateTime.Now.Ticks / 10000,
                Name = Guid.NewGuid().ToString("D"),
                CreateDate = DateTime.Now
            },
            token
        );

        await Task.Delay(1000 * 2, token);

        await responseStream.WriteAsync(
            new UserDto
            {
                Id = (int)DateTime.Now.Ticks / 10000,
                Name = Guid.NewGuid().ToString("D"),
                CreateDate = DateTime.Now
            },
            token
        );
    }
}
