using System;
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
            var (rpcMethodName, methodType, requestDataType, responseDataType) = GetRpcMethodInfo(methodInfo);
            this.RpcMethod = methodInfo;
            this.RpcMethodName = rpcMethodName;
            this.RpcMethodType = methodType;
            this.RequestDataType = requestDataType;
            this.ResponseDataType = responseDataType;

            this.CheckRpcMethodParameterType(methodInfo);
        }

        public MethodInfo RpcMethod { get; }

        public string RpcMethodName { get; }

        public MethodType RpcMethodType { get; }

        public Type RequestDataType { get; }

        public Type ResponseDataType { get; }

        private static (string rpcMethodName, MethodType methodType, Type requestDataType, Type responseDataType) GetRpcMethodInfo(MethodInfo methodInfo)
        {
            var attr = methodInfo.GetCustomAttribute(typeof(RpcMethodAttribute), true) as RpcMethodAttribute;

            return (
                string.IsNullOrWhiteSpace(attr.Name) ? methodInfo.Name : attr.Name,
                attr.MethodType,
                attr.RequestDataType,
                attr.ResponseDataType
            );
        }

        private void CheckRpcMethodParameterType(MethodInfo method)
        {
            if (this.RpcMethodType == MethodType.Unary && method.ReturnType.GenericTypeArguments.Length != 1)
            {
                throw new RpcDefineException("The return value type of RPC method must be Task<T>.");
            }

            if (this.RpcMethodType == MethodType.Unary && method.GetParameters().Length != 2)
            {
                throw new RpcDefineException("The RPC method can only contain two parameters, the first one is the generic TRequest and the other is the System.Threading.CancellationToken type.");
            }

            if (this.RpcMethodType == MethodType.Unary && method.GetParameters()[1].ParameterType != typeof(CancellationToken))
            {
                throw new RpcDefineException("The second parameter of the RPC method must be the System.Threading.CancellationToken type.");
            }
        }
    }
}
