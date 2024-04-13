using System;
using System.Collections;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using GrpcCore = Grpc.Core;

namespace SimpleRpc.Server.Internal;

internal class GrpcHost : IRpcHost
{
    private readonly GrpcCore.Server _server;
    private readonly ILogger<GrpcHost> _logger;

    public GrpcHost(GrpcCore.Server server, ILoggerFactory loggerFactory)
    {
        this._server = server;
        this._logger = loggerFactory.CreateLogger<GrpcHost>();
    }

    public async Task StartAsync()
    {
        await Task.Run(() => {
            this.LogGrpcEndpoints(this._server.GetType());

            this._server.Start();
        }).ConfigureAwait(false);
    }

    public async Task StopAsync()
    {
        await this._server.ShutdownAsync().ConfigureAwait(false);
    }

    private void LogGrpcEndpoints(Type typeOfServer)
    {
        var fieldType = typeOfServer.GetField("callHandlers", BindingFlags.NonPublic | BindingFlags.Instance);
        if (fieldType == null)
        {
            this._logger.LogInformation("No gRPC endpoints found!");
            return;
        }

        var dict = fieldType.GetValue(this._server) as IDictionary;
        if (dict is not null)
        {
            var builder = new StringBuilder();
            builder.AppendLine("------ gRPC Endpoints ------");

            foreach (var endpoint in dict.Keys)
            {
                builder.AppendLine($"{endpoint}");
            }

            this._logger.LogInformation(builder.ToString());
        }
    }
}
