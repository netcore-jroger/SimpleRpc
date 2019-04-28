using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using SimpleRpc.Shared.Internal;
using SimpleRpc.Shared.Serializers;
using GrpcCore = Grpc.Core;

namespace SimpleRpc.Client
{
    public class DefaultRpcChannel : IRpcChannel
    {
        private readonly GrpcCore.DefaultCallInvoker _invoker;
        private readonly ISerializer _serializer;

        public DefaultRpcChannel(IRpcServiceDiscovery rpcServiceDiscovery, ISerializer serializer, IOptions<RpcClientOptions> options)
        {
            var (host, port) = rpcServiceDiscovery == null
                ? (options.Value.Host, options.Value.Port)
                : rpcServiceDiscovery.ResolveAsync().ConfigureAwait(false).GetAwaiter().GetResult();
            var channel = new GrpcCore.Channel(host, port, GrpcCore.ChannelCredentials.Insecure);
            this._invoker = new GrpcCore.DefaultCallInvoker(channel);
            this._serializer = serializer ?? throw new ArgumentNullException(nameof(serializer));
        }

        public async Task<TResponse> CallUnaryMethodAsync<TRequest, TResponse>(TRequest request, string serviceName, string methodName, CancellationToken token)
            where TRequest : class
            where TResponse : class
        {
            var callOptions = new GrpcCore.CallOptions(cancellationToken: token).WithWaitForReady();
            var methodDefinition = this.GetMethodDefinition<TRequest, TResponse>(GrpcCore.MethodType.Unary, serviceName, methodName);
            using (var call = this._invoker.AsyncUnaryCall(methodDefinition, null, callOptions, request))
            {
                var result = await call.ResponseAsync.ConfigureAwait(false);

                return result;
            }
        }

        private GrpcCore.Method<TRequest, TResponse> GetMethodDefinition<TRequest, TResponse>(GrpcCore.MethodType methodType, string serviceName, string methodName)
            where TRequest : class
            where TResponse : class
        {
            return MethodDefinitionGenerator.CreateMethodDefinition<TRequest, TResponse>(methodType, serviceName, methodName, this._serializer);
        }
    }
}
