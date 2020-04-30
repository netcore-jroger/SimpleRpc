using Grpc.Core;
using SimpleRpc.Shared;

namespace SimpleRpc.Server
{
    public abstract class RpcServiceBase : IRpcService
    {
        public ServerCallContext Context { get; internal set; }
    }

    public abstract class RpcServiceBaseServer<TRequest> : RpcServiceBase
    {

        //TODO: ?
        //     private object RequestStream {get; private set;}
        //     void SetAsyncStreamReader<TRequest>(IAsyncStreamReader<TRequest> requestStream);
        //     IAsyncStreamReader<TRequest> GetAsyncStreamReader<TRequest>() => (IAsyncStreamReader<TRequest>)this.RequestStream;
        public IAsyncStreamReader<TRequest> RequestStream { get; internal set; }
    }
}
