using Grpc.Core;
using SimpleRpc.Shared;

namespace SimpleRpc.Server;

public abstract class RpcServiceBase : IRpcService
{
    private object _requestStream;

    public ServerCallContext Context { get; internal set; }

    internal void SetAsyncStreamReader<TRequest>(IAsyncStreamReader<TRequest> requestStream)
    {
        this._requestStream = requestStream;
    }

    public IAsyncStreamReader<TRequest> GetAsyncStreamReader<TRequest>() => (IAsyncStreamReader<TRequest>)this._requestStream;
}
