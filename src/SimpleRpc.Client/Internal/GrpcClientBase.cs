using System;
using System.Threading;
using System.Threading.Tasks;
using SimpleRpc.Shared;

namespace SimpleRpc.Client.Internal
{
    public abstract class GrpcClientBase
    {
        private readonly IRpcChannel _rpcChannel;

        protected GrpcClientBase(IRpcChannel rpcChannel)
        {
            this._rpcChannel = rpcChannel ?? throw new ArgumentNullException(nameof(rpcChannel));
        }

        protected Task<TResponse> CallUnaryMethodAsync<TRequest, TResponse>(TRequest request, string serviceName, string methodName, CancellationToken token)
            where TRequest : class
            where TResponse : class
        {
            return this._rpcChannel.CallUnaryMethodAsync<TRequest, TResponse>(request, serviceName, methodName, token);
        }

        // TODO: change to ClientStreaming<TRequest, TResponse>
        protected ClientStreaming<TRequest, TResponse> AsyncClientStreamingCall<TRequest, TResponse>(string serviceName, string methodName, CancellationToken token)
            where TRequest : class
            where TResponse : class
        {
            var call = this._rpcChannel.AsyncClientStreamingCall<TRequest, TResponse>(serviceName, methodName, token);
            var result = new ClientStreaming<TRequest, TResponse>();
            result.SetAsyncClientStreamingCall(call);

            return result;
        }
    }
}
