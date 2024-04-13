using System.Threading.Tasks;
using GrpcCore = Grpc.Core;

namespace SimpleRpc.Server.Internal;

internal class GrpcHost : IRpcHost
{
    private readonly GrpcCore.Server _server;

    public GrpcHost(GrpcCore.Server server)
    {
        this._server = server;
    }

    public async Task StartAsync()
    {
        await Task.Run(() => {
            this._server.Start();
        }).ConfigureAwait(false);
    }

    public async Task StopAsync()
    {
        await this._server.ShutdownAsync().ConfigureAwait(false);
    }
}
