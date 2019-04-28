using System;
using System.Threading;
using System.Threading.Tasks;
using InterfaceLib;
using SimpleRpc.Server;

namespace ServerSample
{
    public class UserService : RpcServiceBase, IUserService
    {
        public Task<UserDTO> GetUserBy(UserRequest request, CancellationToken token = default)
        {
            return Task.FromResult(new UserDTO
            {
                Id = (int)DateTime.Now.Ticks / 10000,
                Name = Guid.NewGuid().ToString("D") + request.Keyword,
                CreateDate = DateTime.Now
            });
        }
    }
}
