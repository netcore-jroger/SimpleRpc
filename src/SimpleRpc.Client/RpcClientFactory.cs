using System;
using System.Collections.Concurrent;
using System.Reflection;
using SimpleRpc.Client.Internal;
using SimpleRpc.Shared;

namespace SimpleRpc.Client
{
    public static class RpcClientFactory
    {
        private static readonly ConcurrentDictionary<Type, TypeInfo> _proxyClientCache = new ConcurrentDictionary<Type, TypeInfo>();
        private static readonly GrpcClientTypeBuilder _builder = new GrpcClientTypeBuilder();

        public static TService Create<TService>(IRpcChannel rpcChannel)
            where TService : class, IRpcService
        {
            var serviceType = typeof(TService);
            if (!_proxyClientCache.ContainsKey(serviceType) || !_proxyClientCache.TryGetValue(serviceType, out var serviceInstanceTypeInfo))
            {
                serviceInstanceTypeInfo = _builder.Create<TService>();
                _proxyClientCache.TryAdd(serviceType, serviceInstanceTypeInfo);
            }
            
            var instance = Activator.CreateInstance(serviceInstanceTypeInfo, rpcChannel);
            return (TService)instance;
        }
    }
}
