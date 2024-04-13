using System;
using Grpc.Core;
using SimpleRpc.Shared;

namespace SimpleRpc.Server;

public abstract class RpcServiceBase : IRpcService
{
    private object _requestStream;
    private object _responseStream;

    public ServerCallContext Context { get; internal set; }

    internal void SetAsyncStreamReader<TRequest>(IAsyncStreamReader<TRequest> requestStream)
    {
        this._requestStream = requestStream;
    }

    internal void SetServerStreamWriter<TResponse>(IServerStreamWriter<TResponse> responseStream)
    {
        this._responseStream = responseStream;
    }

    public IAsyncStreamReader<TRequest> GetAsyncStreamReader<TRequest>()
    {
        ArgumentNullException.ThrowIfNull(nameof(this._requestStream));

        return this._requestStream as IAsyncStreamReader<TRequest>;
    }

    public IServerStreamWriter<TResponse> GetServerStreamWriter<TResponse>()
    {
        ArgumentNullException.ThrowIfNull(nameof(this._responseStream));

        return this._responseStream as IServerStreamWriter<TResponse>;
    }
}
