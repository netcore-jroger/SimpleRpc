// Copyright (c) JRoger. All Rights Reserved.

using System;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

namespace SimpleRpc.Server.Internal;

internal static class MethodHandlerGenerator
{
    public static Func<TService, TRequest, CancellationToken, Task<TResponse>> GenerateUnaryMethodHandler<TService, TRequest, TResponse>(MethodInfo method)
        where TRequest : class
        where TResponse : class
    {
        var serviceParameter = Expression.Parameter(typeof(TService));
        var requestParameter = Expression.Parameter(typeof(TRequest));
        var ctParameter = Expression.Parameter(typeof(CancellationToken));
        var invocation = Expression.Call(serviceParameter, method, new[] { requestParameter, ctParameter });
        var func = Expression.Lambda<Func<TService, TRequest, CancellationToken, Task<TResponse>>>(
            invocation, false, new[] { serviceParameter, requestParameter, ctParameter }
        )
        .Compile();

        return func;
    }

    public static Func<TService, CancellationToken, Task<TResponse>> GenerateClientStreamingMethodHandler<TService, TResponse>(MethodInfo method)
        where TResponse : class
    {
        var serviceParameter = Expression.Parameter(typeof(TService));
        var ctParameter = Expression.Parameter(typeof(CancellationToken));
        var invocation = Expression.Call(serviceParameter, method, new[] { ctParameter });
        var func = Expression.Lambda<Func<TService, CancellationToken, Task<TResponse>>>(
            invocation, false, new[] { serviceParameter, ctParameter }
        )
        .Compile();

        return func;
    }

    public static Func<TService, TRequest, CancellationToken, Task> GenerateServerStreamingMethodHandler<TService, TRequest>(MethodInfo method)
    {
        var serviceParameter = Expression.Parameter(typeof(TService));
        var requestParameter = Expression.Parameter(typeof(TRequest));
        var ctParameter = Expression.Parameter(typeof(CancellationToken));
        var invocation = Expression.Call(serviceParameter, method, new[] { requestParameter, ctParameter });
        var func = Expression.Lambda<Func<TService, TRequest, CancellationToken, Task>>(
            invocation, false, new[] { serviceParameter, requestParameter, ctParameter }
        )
        .Compile();

        return func;
    }

    public static Func<TService, CancellationToken, Task> GenerateDuplexStreamingMethodHandler<TService, TResponse>(MethodInfo method)
        where TResponse : class
    {
        var serviceParameter = Expression.Parameter(typeof(TService));
        var ctParameter = Expression.Parameter(typeof(CancellationToken));
        var invocation = Expression.Call(serviceParameter, method, new[] { ctParameter });
        var func = Expression.Lambda<Func<TService, CancellationToken, Task>>(
            invocation, false, new[] { serviceParameter, ctParameter }
        )
        .Compile();

        return func;
    }
}
