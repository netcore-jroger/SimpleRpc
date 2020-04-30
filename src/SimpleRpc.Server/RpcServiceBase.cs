using Grpc.Core;
using SimpleRpc.Shared;

namespace SimpleRpc.Server
{
    public abstract class RpcServiceBase : IRpcService
    {
        public ServerCallContext Context { get; internal set; }
    }
}
