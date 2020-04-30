using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Grpc.Core;
using Grpc.Health.V1;
using Grpc.HealthCheck;
using SimpleRpc.Shared.Serializers;
using SimpleRpc.Shared;
using SimpleRpc.Shared.Internal;
using GrpcCoreServer = Grpc.Core.Server;

namespace SimpleRpc.Server.Internal
{
    internal class GrpcHostBuilder : IRpcHostBuilder
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ServerServiceDefinition.Builder _builder;
        private readonly ISerializer _serializer;
        private readonly RpcServerOptions _options;

        public GrpcHostBuilder(IServiceProvider serviceProvider, ISerializer serializer, IOptions<RpcServerOptions> options)
        {
            this._serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
            this._builder = ServerServiceDefinition.CreateBuilder();
            this._serializer = serializer;
            this._options = options.Value;
        }

        public IRpcHostBuilder AddUnaryMethod<TService, TRequest, TResponse> (
            Func<TService, TRequest, CancellationToken, Task<TResponse>> handler,
            string serviceName,
            string methodName
        )
            where TService : class, IRpcService
            where TRequest : class
            where TResponse : class
        {
            this._builder.AddMethod(
                MethodDefinitionGenerator.CreateMethodDefinition<TRequest, TResponse>(MethodType.Unary, serviceName, methodName, this._serializer),
                (request, context) => {
                    using (var scope = this._serviceProvider.CreateScope())
                    {
                        var service = scope.ServiceProvider.GetServices<TService>().First(s => !s.GetType().Name.EndsWith("GrpcClientProxy", StringComparison.OrdinalIgnoreCase));
                        if (service is RpcServiceBase baseService)
                        {
                            baseService.Context = context;
                        }
                        return handler(service, request, context.CancellationToken);
                    }
                }
            );

            return this;
        }

        public IRpcHostBuilder AddClientStreamingMethod<TService, TRequest, TResponse> (
            Func<TService, IAsyncStreamReader<TRequest>, CancellationToken, Task<TResponse>> handler,
            string serviceName,
            string methodName
        )
            where TService : class, IRpcService
            where TRequest : class
            where TResponse : class
        {
            var method = MethodDefinitionGenerator.CreateMethodDefinition<TRequest, TResponse>(MethodType.ClientStreaming, serviceName, methodName, this._serializer);

            this._builder.AddMethod(
                method,
                (requestStream, context) => {
                    using (var scope = this._serviceProvider.CreateScope())
                    {
                        var service = scope.ServiceProvider.GetServices<TService>().First(s => !s.GetType().Name.EndsWith("GrpcClientProxy", StringComparison.OrdinalIgnoreCase));
                        if (service is RpcServiceBaseServer<TRequest> baseService)
                        {
                            baseService.Context = context;
                            baseService.RequestStream = requestStream;
                        }
                        return handler(service, requestStream, context.CancellationToken);
                    }
                }
            );

            return this;
        }

        public IRpcHost Build()
        {
            var healthService = new HealthServiceImpl();
            healthService.SetStatus(RpcConfigInformation.RpcHealthCheckPath, HealthCheckResponse.Types.ServingStatus.Serving);
            var server = new GrpcCoreServer {
                Ports = {
                    new ServerPort("0.0.0.0", this._options.Port, ServerCredentials.Insecure)
                },
                Services = {
                    Health.BindService(healthService),
                    this._builder.Build()
                }
            };

            return new GrpcHost(server);
        }
    }
}
