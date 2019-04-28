using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using SimpleRpc.Shared.Serializers;
using SimpleRpc.Shared;

namespace SimpleRpc.Client
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddRpcClient(this IServiceCollection services, IConfiguration configuration)
        {
            services.TryAddSingleton<ISerializer, ProtoBufSerializer>();
            services.TryAddScoped<IRpcChannel, DefaultRpcChannel>();
            //services.TryAddScoped<IRpcServiceDiscovery, ConsulRpcServiceDiscovery>();
            services.TryAddScoped<IRpcServiceDiscovery, DefaultRpcServiceDiscovery>();

            services.Configure<RpcClientOptions>(configuration.GetSection(RpcConfigInformation.RpcClientConfigSectionName));

            return services;
        }

        public static IServiceCollection AddRpcClientService<TServiceInterface>(this IServiceCollection services)
            where TServiceInterface : class, IRpcService
        {
            services.AddScoped<TServiceInterface>(serviceProvider => {
                var rpcChannel = serviceProvider.GetRequiredService<IRpcChannel>();

                return RpcClientFactory.Create<TServiceInterface>(rpcChannel);
            });

            return services;
        }
    }
}
