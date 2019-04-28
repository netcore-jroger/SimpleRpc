using InterfaceLib;
using SimpleRpc.Server;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace ServerSample
{
    public class UserService : RpcServiceBase, IUserService
    {
        public Task<UserDTO> GetUserBy(int userId, CancellationToken token = default)
        {
            return Task.FromResult(new UserDTO {
                Id = (int)DateTime.Now.Ticks / 10000,
                Name = Guid.NewGuid().ToString("D")
            });
        }
    }
}
