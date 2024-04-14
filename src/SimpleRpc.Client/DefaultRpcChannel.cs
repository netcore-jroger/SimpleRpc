// Copyright (c) JRoger. All Rights Reserved.

using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using Grpc.Core;
using SimpleRpc.Shared.Internal;
using SimpleRpc.Shared.Serializers;

namespace SimpleRpc.Client;

public class DefaultRpcChannel : IRpcChannel
{
    private readonly DefaultCallInvoker _invoker;
    private readonly ISerializer _serializer;
    private readonly string _host;

    public DefaultRpcChannel(IRpcServiceDiscovery rpcServiceDiscovery, ISerializer serializer, IOptions<RpcClientOptions> options)
    {
        var (host, port) = rpcServiceDiscovery == null
            ? (options.Value.Host, options.Value.Port)
            : rpcServiceDiscovery.ResolveAsync().ConfigureAwait(false).GetAwaiter().GetResult();
        var channel = new Channel(host, port, ChannelCredentials.Insecure);
        this._host = host;
        this._invoker = new DefaultCallInvoker(channel);
        this._serializer = serializer ?? throw new ArgumentNullException(nameof(serializer));
    }

    public async Task<TResponse> CallUnaryMethodAsync<TRequest, TResponse>(TRequest request, string serviceName, string methodName, CancellationToken token)
        where TRequest : class
        where TResponse : class
    {
        var callOptions = new CallOptions(cancellationToken: token).WithWaitForReady();
        var methodDefinition = this.GetMethodDefinition<TRequest, TResponse>(MethodType.Unary, serviceName, methodName);

        using var call = this._invoker.AsyncUnaryCall(methodDefinition, null, callOptions, request);
        var result = await call.ResponseAsync.ConfigureAwait(false);

        return result;
    }

    public AsyncClientStreamingCall<TRequest, TResponse> AsyncClientStreamingCall<TRequest, TResponse>(string serviceName, string methodName, CancellationToken token)
        where TRequest : class
        where TResponse : class
    {
        var callOptions = new CallOptions(cancellationToken: token).WithWaitForReady();
        var methodDefinition = this.GetMethodDefinition<TRequest, TResponse>(MethodType.ClientStreaming, serviceName, methodName);
        var result = this._invoker.AsyncClientStreamingCall<TRequest, TResponse>(methodDefinition, this._host, callOptions);

        return result;
    }

    public AsyncServerStreamingCall<TResponse> AsyncServerStreamingCall<TRequest, TResponse>(string serviceName, string methodName, TRequest request, CancellationToken token)
        where TRequest : class
        where TResponse : class
    {
        var callOptions = new CallOptions(cancellationToken: token).WithWaitForReady();
        var methodDefinition = this.GetMethodDefinition<TRequest, TResponse>(MethodType.ServerStreaming, serviceName, methodName);
        var result = this._invoker.AsyncServerStreamingCall<TRequest, TResponse>(methodDefinition, this._host, callOptions, request);

        return result;
    }

    public AsyncDuplexStreamingCall<TRequest, TResponse> AsyncDuplexStreamingCall<TRequest, TResponse>(string serviceName, string methodName, CancellationToken token)
        where TRequest : class
        where TResponse : class
    {
        var callOptions = new CallOptions(cancellationToken: token).WithWaitForReady();
        var methodDefinition = this.GetMethodDefinition<TRequest, TResponse>(MethodType.DuplexStreaming, serviceName, methodName);
        var result = this._invoker.AsyncDuplexStreamingCall<TRequest, TResponse>(methodDefinition, this._host, callOptions);

        return result;
    }

    private Method<TRequest, TResponse> GetMethodDefinition<TRequest, TResponse>(MethodType methodType, string serviceName, string methodName)
        where TRequest : class
        where TResponse : class
    {
        return MethodDefinitionGenerator.CreateMethodDefinition<TRequest, TResponse>(methodType, serviceName, methodName, this._serializer);
    }
}
