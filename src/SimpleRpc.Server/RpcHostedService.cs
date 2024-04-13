using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace SimpleRpc.Server;

internal class RpcHostedService : IHostedService
{
    private readonly IRpcHost _host;
    private readonly IRpcServiceRegister _rpcServiceRegister;
    private readonly RpcServerOptions _options;
    private readonly ILogger<RpcHostedService> _logger;

    public RpcHostedService(IRpcHost host, IOptions<RpcServerOptions> options, IRpcServiceRegister rpcServiceRegister, ILoggerFactory loggerFactory)
    {
        this._host = host ?? throw new ArgumentNullException(nameof(host));
        this._rpcServiceRegister = rpcServiceRegister;
        this._options = options.Value;
        this._logger = loggerFactory.CreateLogger<RpcHostedService>();
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        this._logger.LogInformation("------ Starting RPC hosted service. ------");

        await this._host.StartAsync().ConfigureAwait(false);

        this._logger.LogInformation($"------ RPC hosted service started at 0.0.0.0:{this._options.Port}. ------");
        
        await this._rpcServiceRegister.RegisterAsync();
    }

    public async Task StopAsync(CancellationToken cancellationToken)
    {
        this._logger.LogInformation("------ Stopping RPC hosted service. ------");

        await this._host.StopAsync().ConfigureAwait(false);

        this._logger.LogInformation("------ RPC hosted service stoped. ------");

        await this._rpcServiceRegister.UnregisterAsync();
    }
}
