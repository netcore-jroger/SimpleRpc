using System;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Threading;
using SimpleRpc.Shared;
using SimpleRpc.Shared.ServiceAnnotations;

namespace SimpleRpc.Server.Internal
{
    internal static class GrpcHostBuilderExtensions
    {
        private static readonly MethodInfo _handlerGenerator = typeof(MethodHandlerGenerator).GetMethod(nameof(MethodHandlerGenerator.GenerateUnaryMethodHandler));
        private static readonly MethodInfo _addUnaryMethod = typeof(GrpcHostBuilder).GetMethod(nameof(GrpcHostBuilder.AddUnaryMethod), BindingFlags.Public | BindingFlags.Instance);

        public static IRpcHostBuilder AddUnaryMethods(this IRpcHostBuilder builder, Type serviceType)
        {
            var serviceName = ((RpcServiceAttribute)serviceType.GetCustomAttribute(typeof(RpcServiceAttribute)))?.Name;
            if (string.IsNullOrWhiteSpace(serviceName)) serviceName = serviceType.Name;

            foreach (var method in serviceType.GetMethods().Where(_ => _.GetCustomAttribute(typeof(RpcMethodAttribute), true) != null))
            {
                CheckRpcMethodParameterType(method);

                var requestType = method.GetParameters()[0].ParameterType;
                var responseType = method.ReturnType.GenericTypeArguments[0];

                var handlerGenerator = _handlerGenerator.MakeGenericMethod(serviceType, requestType, responseType);
                var handler = handlerGenerator.Invoke(null, new[] { method });

                var methodName = GetMethodName(method);
                var addUnaryMethod = _addUnaryMethod.MakeGenericMethod(serviceType, requestType, responseType);
                addUnaryMethod.Invoke(builder, new[] { handler, serviceName, methodName });
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
}
