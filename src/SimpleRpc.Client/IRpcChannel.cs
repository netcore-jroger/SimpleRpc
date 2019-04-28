using System.Threading;
using System.Threading.Tasks;

namespace SimpleRpc.Client
{
    public interface IRpcChannel
    {
         Task<TResponse> CallUnaryMethodAsync<TRequest, TResponse>(TRequest request, string serviceName, string methodName, CancellationToken token)
            where TRequest : class
            where TResponse : class;
    }
}
