// Copyright (c) JRoger. All Rights Reserved.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using SimpleRpc.Shared.ServiceAnnotations;

namespace SimpleRpc.Shared.Description
{
    public sealed class RpcServiceDescription
    {
        public RpcServiceDescription(Type rpcServiceType)
        {
            this.RpcServiceType = rpcServiceType ?? throw new ArgumentNullException(nameof(rpcServiceType));
            this.RpcServiceName = GetRpcServiceName(rpcServiceType);
            this.RpcMethods = GetRpcMethods(rpcServiceType);
        }

        public string RpcServiceName { get; }

        public Type RpcServiceType { get; }

        public List<RpcMethodDescription> RpcMethods { get; }

        private static string GetRpcServiceName(MemberInfo rpcServiceType)
        {
            var rpcServiceName = ((RpcServiceAttribute)rpcServiceType.GetCustomAttribute(typeof(RpcServiceAttribute)))?.Name;
            if (string.IsNullOrWhiteSpace(rpcServiceName))
            {
                rpcServiceName = rpcServiceType.Name;
            }

            return rpcServiceName;
        }

        private static List<RpcMethodDescription> GetRpcMethods(Type rpcServiceType)
        {
            return rpcServiceType.GetTypeInfo()
                .GetMethods(BindingFlags.Public | BindingFlags.Instance)
                .Where(m => m.GetCustomAttribute(typeof(RpcMethodAttribute), true) != null)
                .Select(m => new RpcMethodDescription(m))
                .ToList();
        }
    }
}
