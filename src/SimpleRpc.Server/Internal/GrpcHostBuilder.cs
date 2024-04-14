// Copyright (c) JRoger. All Rights Reserved.

using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Grpc.Core;
using Grpc.Health.V1;
using Grpc.HealthCheck;
using SimpleRpc.Shared.Serializers;
using SimpleRpc.Shared;
using SimpleRpc.Shared.Internal;
using GrpcCoreServer = Grpc.Core.Server;

namespace SimpleRpc.Server.Internal;

internal class GrpcHostBuilder : IRpcHostBuilder
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ServerServiceDefinition.Builder _builder;
    private readonly ISerializer _serializer;
    private readonly RpcServerOptions _options;
    private readonly ILoggerFactory _loggerFactory;
    private readonly ILogger<GrpcHostBuilder> _logger;

    public GrpcHostBuilder(IServiceProvider serviceProvider, ISerializer serializer, IOptions<RpcServerOptions> options, ILoggerFactory loggerFactory)
    {
        this._serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
        this._builder = ServerServiceDefinition.CreateBuilder();
        this._serializer = serializer;
        this._options = options.Value;
        this._loggerFactory = loggerFactory;
        this._logger = loggerFactory.CreateLogger<GrpcHostBuilder>();
    }

    public IRpcHostBuilder AddUnaryMethod<TService, TRequest, TResponse>(
        Func<TService, TRequest, CancellationToken, Task<TResponse>> handler,
        string serviceName,
        string methodName
    )
        where TService : class, IRpcService
        where TRequest : class
        where TResponse : class
    {
        var method = MethodDefinitionGenerator.CreateMethodDefinition<TRequest, TResponse>(MethodType.Unary, serviceName, methodName, this._serializer);

        this._builder.AddMethod(method, UnaryServerMethodDelegate);

        // See delegate: UnaryServerMethod<TRequest, TResponse>
        Task<TResponse> UnaryServerMethodDelegate(TRequest request, ServerCallContext context)
        {
            using ( var scope = this._serviceProvider.CreateScope() )
            {
                var service = scope.ServiceProvider.GetServices<TService>().First(s => !s.GetType().Name.EndsWith("GrpcClientProxy", StringComparison.OrdinalIgnoreCase));
                if ( service is RpcServiceBase baseService )
                {
                    baseService.Context = context;
                }
                else
                {
                    throw new NotImplementedException($"Service type {typeof(TService)} must implement abstract class {typeof(RpcServiceBase)}");
                }

                this._logger.LogInformation($"Request gRPC endpoint: {context.Method}");

                return handler(service, request, context.CancellationToken);
            }
        }

        return this;
    }

    public IRpcHostBuilder AddClientStreamingMethod<TService, TRequest, TResponse>(
        Func<TService, CancellationToken, Task<TResponse>> handler,
        string serviceName,
        string methodName
    )
        where TService : class, IRpcService
        where TRequest : class
        where TResponse : class
    {
        var method = MethodDefinitionGenerator.CreateMethodDefinition<TRequest, TResponse>(MethodType.ClientStreaming, serviceName, methodName, this._serializer);

        this._builder.AddMethod(method, ClientStreamingServerMethodDelegate);

        // See delegate: ClientStreamingServerMethod<TRequest, TResponse>
        Task<TResponse> ClientStreamingServerMethodDelegate(IAsyncStreamReader<TRequest> requestStream, ServerCallContext context)
        {
            using ( var scope = this._serviceProvider.CreateScope() )
            {
                var service = scope.ServiceProvider.GetServices<TService>().First(s => !s.GetType().Name.EndsWith("GrpcClientProxy", StringComparison.OrdinalIgnoreCase));
                if ( service is RpcServiceBase baseService )
                {
                    baseService.Context = context;
                    baseService.SetAsyncStreamReader(requestStream);
                }
                else
                {
                    throw new NotImplementedException($"Service type {typeof(TService)} must implement abstract class {typeof(RpcServiceBase)}");
                }

                this._logger.LogInformation($"Request gRPC endpoint: {context.Method}");

                return handler(service, context.CancellationToken);
            }
        }

        return this;
    }

    public IRpcHostBuilder AddServerStreamingMethod<TService, TRequest, TResponse>(
        Func<TService, TRequest, CancellationToken, Task> handler,
        string serviceName,
        string methodName
    )
        where TService : class, IRpcService
        where TRequest : class
        where TResponse : class
    {
        var method = MethodDefinitionGenerator.CreateMethodDefinition<TRequest, TResponse>(MethodType.ServerStreaming, serviceName, methodName, this._serializer);

        this._builder.AddMethod(method, ServerStreamingServerMethodDelegate);

        // See delegate: ServerStreamingServerMethod<TRequest, TResponse>
        Task ServerStreamingServerMethodDelegate(TRequest request, IServerStreamWriter<TResponse> responseStream, ServerCallContext context)
        {
            using ( var scope = this._serviceProvider.CreateScope() )
            {
                var service = scope.ServiceProvider.GetServices<TService>().First(s => !s.GetType().Name.EndsWith("GrpcClientProxy", StringComparison.OrdinalIgnoreCase));
                if ( service is RpcServiceBase baseService )
                {
                    baseService.Context = context;
                    baseService.SetServerStreamWriter(responseStream);
                }
                else
                {
                    throw new NotImplementedException($"Service type {typeof(TService)} must implement abstract class {typeof(RpcServiceBase)}");
                }

                this._logger.LogInformation($"Request gRPC endpoint: {context.Method}");

                return handler(service, request, context.CancellationToken);
            }
        }

        return this;
    }

    public IRpcHostBuilder AddDuplexStreamingMethod<TService, TRequest, TResponse>(
        Func<TService, CancellationToken, Task> handler,
        string serviceName,
        string methodName
    )
        where TService : class, IRpcService
        where TRequest : class
        where TResponse : class
    {
        var method = MethodDefinitionGenerator.CreateMethodDefinition<TRequest, TResponse>(MethodType.DuplexStreaming, serviceName, methodName, this._serializer);

        this._builder.AddMethod(method, DuplexStreamingServerMethodDelegate);

        // See delegate: DuplexStreamingServerMethod<TRequest, TResponse>
        Task DuplexStreamingServerMethodDelegate(IAsyncStreamReader<TRequest> requestStream, IServerStreamWriter<TResponse> responseStream, ServerCallContext context)
        {
            using ( var scope = this._serviceProvider.CreateScope() )
            {
                var service = scope.ServiceProvider.GetServices<TService>().First(s => !s.GetType().Name.EndsWith("GrpcClientProxy", StringComparison.OrdinalIgnoreCase));
                if ( service is RpcServiceBase baseService )
                {
                    baseService.Context = context;
                    baseService.SetAsyncStreamReader(requestStream);
                    baseService.SetServerStreamWriter(responseStream);
                }
                else
                {
                    throw new NotImplementedException($"Service type {typeof(TService)} must implement abstract class {typeof(RpcServiceBase)}");
                }

                this._logger.LogInformation($"Request gRPC endpoint: {context.Method}");

                return handler(service, context.CancellationToken);
            }
        }

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

        return new GrpcHost(server, this._loggerFactory);
    }
}
