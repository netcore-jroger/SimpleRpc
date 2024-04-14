// Copyright (c) JRoger. All Rights Reserved.

using System;
using System.Threading;
using System.Threading.Tasks;
using Grpc.Core;
using SimpleRpc.Shared;

namespace SimpleRpc.Client.Internal;

public abstract class GrpcClientBase
{
    private readonly IRpcChannel _rpcChannel;

    protected GrpcClientBase(IRpcChannel rpcChannel)
    {
        this._rpcChannel = rpcChannel ?? throw new ArgumentNullException(nameof(rpcChannel));
    }

    protected Task<TResponse> CallUnaryMethodAsync<TRequest, TResponse>(TRequest request, string serviceName, string methodName, CancellationToken token)
        where TRequest : class
        where TResponse : class
    {
        return this._rpcChannel.CallUnaryMethodAsync<TRequest, TResponse>(request, serviceName, methodName, token);
    }

    protected ClientStreaming<TRequest, TResponse> AsyncClientStreamingCall<TRequest, TResponse>(string serviceName, string methodName, CancellationToken token)
        where TRequest : class
        where TResponse : class
    {
        var call = this._rpcChannel.AsyncClientStreamingCall<TRequest, TResponse>(serviceName, methodName, token);
        var result = new ClientStreaming<TRequest, TResponse>();
        result.SetAsyncClientStreamingCall(call);

        return result;
    }

    protected AsyncServerStreamingCall<TResponse> AsyncServerStreamingCall<TRequest, TResponse>(string serviceName, string methodName, TRequest request, CancellationToken token)
        where TRequest : class
        where TResponse : class
    {
        var call = this._rpcChannel.AsyncServerStreamingCall<TRequest, TResponse>(serviceName, methodName, request, token);

        return call;
    }

    protected AsyncDuplexStreamingCall<TRequest, TResponse> AsyncDuplexStreamingCall<TRequest, TResponse>(string serviceName, string methodName, CancellationToken token)
        where TRequest : class
        where TResponse : class
    {
        var call = this._rpcChannel.AsyncDuplexStreamingCall<TRequest, TResponse>(serviceName, methodName, token);

        return call;
    }
}
