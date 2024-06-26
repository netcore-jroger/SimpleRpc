// Copyright (c) JRoger. All Rights Reserved.

using System;
using System.Threading;
using System.Threading.Tasks;
using SimpleRpc.Shared;

namespace SimpleRpc.Server;

public interface IRpcHostBuilder
{
    /// <summary>
    /// 添加 UnaryMethod RPC 方法
    /// </summary>
    /// <param name="handler"></param>
    /// <param name="serviceName"></param>
    /// <param name="methodName"></param>
    /// <typeparam name="TService"></typeparam>
    /// <typeparam name="TRequest"></typeparam>
    /// <typeparam name="TResponse"></typeparam>
    /// <returns></returns>
    IRpcHostBuilder AddUnaryMethod<TService, TRequest, TResponse>(
        Func<TService, TRequest, CancellationToken, Task<TResponse>> handler,
        string serviceName,
        string methodName
    )
        where TService : class, IRpcService
        where TRequest : class
        where TResponse : class;

    /// <summary>
    /// 添加 ClientStreamingMethod RPC 方法
    /// </summary>
    /// <param name="handler"></param>
    /// <param name="serviceName"></param>
    /// <param name="methodName"></param>
    /// <typeparam name="TService"></typeparam>
    /// <typeparam name="TRequest"></typeparam>
    /// <typeparam name="TResponse"></typeparam>
    /// <returns></returns>
    IRpcHostBuilder AddClientStreamingMethod<TService, TRequest, TResponse>(
        Func<TService, CancellationToken, Task<TResponse>> handler,
        string serviceName,
        string methodName
    )
        where TService : class, IRpcService
        where TRequest : class
        where TResponse : class;

    /// <summary>
    /// 添加 ServerStreamingMethod RPC 方法
    /// </summary>
    /// <typeparam name="TService"></typeparam>
    /// <typeparam name="TRequest"></typeparam>
    /// <typeparam name="TResponse"></typeparam>
    /// <param name="handler"></param>
    /// <param name="serviceName"></param>
    /// <param name="methodName"></param>
    /// <returns></returns>
    IRpcHostBuilder AddServerStreamingMethod<TService, TRequest, TResponse>(
        Func<TService, TRequest, CancellationToken, Task> handler,
        string serviceName,
        string methodName
    )
        where TService : class, IRpcService
        where TRequest : class
        where TResponse : class;

    /// <summary>
    /// 添加 DuplexStreamingMethod RPC 方法
    /// </summary>
    /// <typeparam name="TService"></typeparam>
    /// <typeparam name="TRequest"></typeparam>
    /// <typeparam name="TResponse"></typeparam>
    /// <param name="handler"></param>
    /// <param name="serviceName"></param>
    /// <param name="methodName"></param>
    /// <returns></returns>
    IRpcHostBuilder AddDuplexStreamingMethod<TService, TRequest, TResponse>(
        Func<TService, CancellationToken, Task> handler,
        string serviceName,
        string methodName
    )
        where TService : class, IRpcService
        where TRequest : class
        where TResponse : class;

    /// <summary>
    /// 创建 Rpc 宿主服务.
    /// </summary>
    /// <returns></returns>
    IRpcHost Build();
}
