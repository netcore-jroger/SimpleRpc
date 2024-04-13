using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Grpc.Core;
using SimpleRpc.Shared;
using SimpleRpc.Shared.Description;
using SimpleRpc.Shared.ServiceAnnotations;

namespace SimpleRpc.Server.Internal;

internal static class GrpcHostBuilderExtensions
{
    // Unary
    private static readonly MethodInfo _unaryHandlerGenerator = typeof(MethodHandlerGenerator).GetMethod(nameof(MethodHandlerGenerator.GenerateUnaryMethodHandler));
    private static readonly MethodInfo _addUnaryMethod = typeof(GrpcHostBuilder).GetMethod(nameof(GrpcHostBuilder.AddUnaryMethod), BindingFlags.Public | BindingFlags.Instance);

    // ClientStreaming
    private static readonly MethodInfo _clientStreamingHandlerGenerator = typeof(MethodHandlerGenerator).GetMethod(nameof(MethodHandlerGenerator.GenerateClientStreamingMethodHandler));
    private static readonly MethodInfo _addClientStreamingMethod = typeof(GrpcHostBuilder).GetMethod(nameof(GrpcHostBuilder.AddClientStreamingMethod), BindingFlags.Public | BindingFlags.Instance);

    // ServerStreaming
    private static readonly MethodInfo _serverStreamingHandlerGenerator = typeof(MethodHandlerGenerator).GetMethod(nameof(MethodHandlerGenerator.GenerateServerStreamingMethodHandler));
    private static readonly MethodInfo _addServerStreamingMethod = typeof(GrpcHostBuilder).GetMethod(nameof(GrpcHostBuilder.AddServerStreamingMethod), BindingFlags.Public | BindingFlags.Instance);

    public static IRpcHostBuilder RegisterRpcService(this IRpcHostBuilder builder, List<RpcServiceDescription> rpcServiceDescriptions)
    {
        if (rpcServiceDescriptions.Count == 0)
        {
            return builder;
        }

        foreach (var rpcServiceDescription in rpcServiceDescriptions)
        {
            foreach (var rpcMethodDescription in rpcServiceDescription.RpcMethods)
            {
                var requestType = rpcMethodDescription.RpcMethod.GetParameters()[0].ParameterType;

                switch (rpcMethodDescription.RpcMethodType)
                {
                    case MethodType.Unary:
                        var unaryResponseType = rpcMethodDescription.RpcMethod.ReturnType.GenericTypeArguments[0];
                        var unaryHandlerGenerator = _unaryHandlerGenerator.MakeGenericMethod(rpcServiceDescription.RpcServiceType, requestType, unaryResponseType);
                        var unaryHandler = unaryHandlerGenerator.Invoke(null, new[] { rpcMethodDescription.RpcMethod });

                        var addUnaryMethod = _addUnaryMethod.MakeGenericMethod(rpcServiceDescription.RpcServiceType, requestType, unaryResponseType);
                        addUnaryMethod.Invoke(builder, new[] { unaryHandler, rpcServiceDescription.RpcServiceName, rpcMethodDescription.RpcMethodName });
                        break;

                    case MethodType.ClientStreaming:
                        var clientStreamingResponseType = rpcMethodDescription.RpcMethod.ReturnType.GenericTypeArguments[0];
                        // Func<TService, CancellationToken, Task<TResponse>>
                        var clientStreamingHandlerGenerator = _clientStreamingHandlerGenerator.MakeGenericMethod(rpcServiceDescription.RpcServiceType, clientStreamingResponseType);
                        var clientStreamingHandler = clientStreamingHandlerGenerator.Invoke(null, new[] { rpcMethodDescription.RpcMethod });

                        var addClientStreamingMethod = _addClientStreamingMethod.MakeGenericMethod(rpcServiceDescription.RpcServiceType, rpcMethodDescription.RequestDataType, clientStreamingResponseType);
                        addClientStreamingMethod.Invoke(builder, new[] { clientStreamingHandler, rpcServiceDescription.RpcServiceName, rpcMethodDescription.RpcMethodName });
                        break;

                    case MethodType.ServerStreaming:
                        // Func<TService, CancellationToken, Task>
                        var serverStreamingHandlerGenerator = _serverStreamingHandlerGenerator.MakeGenericMethod(rpcServiceDescription.RpcServiceType, rpcMethodDescription.RequestDataType);
                        var serverStreamingHandler = serverStreamingHandlerGenerator.Invoke(null, new[] { rpcMethodDescription.RpcMethod });

                        var addServerStreamingMethod = _addServerStreamingMethod.MakeGenericMethod(rpcServiceDescription.RpcServiceType, rpcMethodDescription.RequestDataType, rpcMethodDescription.ResponseDataType);
                        addServerStreamingMethod.Invoke(builder, new[] { serverStreamingHandler, rpcServiceDescription.RpcServiceName, rpcMethodDescription.RpcMethodName });
                        break;

                    default:
                        throw new NotSupportedException($"unsupport gRPC MethodType: {rpcMethodDescription.RpcMethodType}");
                }
            }
        }

        return builder;
    }

    public static IRpcHostBuilder AddUnaryMethods(this IRpcHostBuilder builder, Type serviceType)
    {
        var serviceName = ((RpcServiceAttribute)serviceType.GetCustomAttribute(typeof(RpcServiceAttribute)))?.Name;
        if (string.IsNullOrWhiteSpace(serviceName))
        {
            serviceName = serviceType.Name;
        }

        foreach (var method in serviceType.GetMethods().Where(_ => _.GetCustomAttribute(typeof(RpcMethodAttribute), true) != null))
        {
            CheckRpcMethodParameterType(method);

            var requestType = method.GetParameters()[0].ParameterType;
            var responseType = method.ReturnType.GenericTypeArguments[0];

            var handlerGenerator = _unaryHandlerGenerator.MakeGenericMethod(serviceType, requestType, responseType);
            var handler = handlerGenerator.Invoke(null, new[] { method });

            var methodName = GetMethodName(method);
            var addUnaryMethod = _addUnaryMethod.MakeGenericMethod(serviceType, requestType, responseType);

            addUnaryMethod.Invoke(builder, new[] { handler, serviceName, methodName });
        }

        return builder;
    }

    public static IRpcHostBuilder AddClientStreamingMethods(this IRpcHostBuilder builder, Type serviceType)
    {
        var serviceName = ((RpcServiceAttribute)serviceType.GetCustomAttribute(typeof(RpcServiceAttribute)))?.Name;
        if (string.IsNullOrWhiteSpace(serviceName)) serviceName = serviceType.Name;

        foreach (var method in serviceType.GetMethods().Where(_ => _.GetCustomAttribute(typeof(RpcMethodAttribute), true) != null))
        {
            CheckRpcMethodParameterType(method);

            var requestType = method.GetParameters()[0].ParameterType;
            var responseType = method.ReturnType.GenericTypeArguments[0];

            var handlerGenerator = _unaryHandlerGenerator.MakeGenericMethod(serviceType, requestType, responseType);
            var handler = handlerGenerator.Invoke(null, new[] { method });

            var methodName = GetMethodName(method);
            var addClientStreamingMethod = _addClientStreamingMethod.MakeGenericMethod(serviceType, requestType, responseType);

            addClientStreamingMethod.Invoke(builder, new[] { handler, serviceName, methodName });
        }

        return builder;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static string GetMethodName(MethodInfo method)
    {
        var methodName = ((RpcMethodAttribute)method.GetCustomAttribute(typeof(RpcMethodAttribute)))?.Name;
        if (string.IsNullOrWhiteSpace(methodName)) methodName = method.Name;

        return methodName;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void CheckRpcMethodParameterType(MethodInfo method)
    {
        if (method.ReturnType.GenericTypeArguments.Length != 1)
        {
            throw new RpcDefineException("The return value type of RPC method must be Task<T>.");
        }

        if (method.GetParameters().Length != 2)
        {
            throw new RpcDefineException("The RPC method can only contain two parameters, the first one is the generic TRequest and the other is the System.Threading.CancellationToken type.");
        }

        if (method.GetParameters()[1].ParameterType != typeof(CancellationToken))
        {
            throw new RpcDefineException("The second parameter of the RPC method must be the System.Threading.CancellationToken type.");
        }
    }
}
