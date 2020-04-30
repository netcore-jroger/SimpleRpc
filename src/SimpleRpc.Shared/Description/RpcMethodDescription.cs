using System.Reflection;
using System.Threading;
using Grpc.Core;
using SimpleRpc.Shared.ServiceAnnotations;

namespace SimpleRpc.Shared.Description
{
    public sealed class RpcMethodDescription
    {
        public RpcMethodDescription(MethodInfo methodInfo)
        {
            CheckRpcMethodParameterType(methodInfo);

            this.RpcMethod = methodInfo;
            var (rpcMethodName, methodType) = GetRpcMethodInfo(methodInfo);
            this.RpcMethodName = rpcMethodName;
            this.RpcMethodType = methodType;
        }

        public MethodInfo RpcMethod { get; }

        public string RpcMethodName { get; }

        public MethodType RpcMethodType { get; }

        private static (string rpcMethodName, MethodType methodType) GetRpcMethodInfo(MethodInfo methodInfo)
        {
            var attr = methodInfo.GetCustomAttribute(typeof(RpcMethodAttribute), true) as RpcMethodAttribute;

            return (
                string.IsNullOrWhiteSpace(attr.Name) ? methodInfo.Name : attr.Name,
                attr.MethodType
            );
        }

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
