using System;
using System.Threading;
using System.Threading.Tasks;

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
    }
}
