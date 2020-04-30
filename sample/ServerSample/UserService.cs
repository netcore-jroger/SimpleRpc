using System;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using InterfaceLib;
using Microsoft.Extensions.Logging;
using SimpleRpc.Server;

namespace ServerSample
{
    public class UserService : RpcServiceBase, IUserService
    {
        private readonly ILogger<UserService> _logger;

        public UserService(ILoggerFactory loggerFactory)
        {
            this._logger = loggerFactory.CreateLogger<UserService>();
        }

        public Task<UserDTO> GetUserBy(UserRequest request, CancellationToken token = default)
        {
            this._logger.LogInformation($"Receive client message：{JsonSerializer.Serialize(request)}");

            return Task.FromResult(new UserDTO
            {
                Id = (int)DateTime.Now.Ticks / 10000,
                Name = Guid.NewGuid().ToString("D") + request.Keyword,
                CreateDate = DateTime.Now
            });
        }

        public Task<UserDTO> TestClientStreaming(UserRequest request, CancellationToken token = default)
        {
            this._logger.LogInformation($"Receive client message：{JsonSerializer.Serialize(request)}");

            
            return Task.FromResult(new UserDTO {
                Id = (int)DateTime.Now.Ticks / 10000,
                Name = Guid.NewGuid().ToString("D") + request.Keyword,
                CreateDate = DateTime.Now
            });
        }
    }
}
