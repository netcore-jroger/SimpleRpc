using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SimpleRpc.Shared.Serializers;
using SimpleRpc.Server.Internal;

namespace SimpleRpc.Server;

public static class ServiceCollectionExtensions
{
    /// <summary>
    /// 添加 gRPC 服务。
    /// </summary>
    /// <param name="services"></param>
    /// <param name="configure"></param>
    /// <returns></returns>
    public static IServiceCollection AddRpcServer(this IServiceCollection services, Action<RpcServerOptions> configure)
    {
        services.Configure<RpcServerOptions>(configure);
        services.TryAddSingleton<IRpcServiceTypeFinder, DefaultRpcServiceTypeFinder>();
        services.TryAddSingleton<ISerializer, ProtoBufSerializer>();
        services.TryAddSingleton<IRpcHostBuilder, GrpcHostBuilder>();

        // Consul 服务注册功能
        //services.TryAddSingleton<IRpcServiceRegister, ConsulRpcServiceRegister>();
        services.TryAddSingleton<IRpcServiceRegister, NoopRpcServiceRegister>();

        services.AddSingleton<IRpcHost>(
            serviceProvider => {
                return GrpcHostFactory.Create(serviceProvider);
            }
        );
        services.AddSingleton<IHostedService>(provider => {
            return new RpcHostedService(
                provider.GetRequiredService<IRpcHost>(),
                provider.GetRequiredService<IOptions<RpcServerOptions>>(),
                provider.GetService<IRpcServiceRegister>(),
                provider.GetRequiredService<ILoggerFactory>()
            );
        });

        return services;
    }
}
