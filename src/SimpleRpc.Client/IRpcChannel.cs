// Copyright (c) JRoger. All Rights Reserved.

using System.Threading;
using System.Threading.Tasks;
using Grpc.Core;

namespace SimpleRpc.Client;

public interface IRpcChannel
{
     Task<TResponse> CallUnaryMethodAsync<TRequest, TResponse>(TRequest request, string serviceName, string methodName, CancellationToken token)
        where TRequest : class
        where TResponse : class;

    AsyncClientStreamingCall<TRequest, TResponse> AsyncClientStreamingCall<TRequest, TResponse>(string serviceName, string methodName, CancellationToken token)
        where TRequest : class
        where TResponse : class;

    AsyncServerStreamingCall<TResponse> AsyncServerStreamingCall<TRequest, TResponse>(string serviceName, string methodName, TRequest request, CancellationToken token)
        where TRequest : class
        where TResponse : class;
}
