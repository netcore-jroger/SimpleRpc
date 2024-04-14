// Copyright (c) JRoger. All Rights Reserved.

using System.Threading.Tasks;
using Microsoft.Extensions.Options;

namespace SimpleRpc.Client;

/// <summary>
/// RPC 服务发现
/// </summary>
public interface IRpcServiceDiscovery
{
    /// <summary>
    /// 获取 RPC 服务主机和端口。
    /// </summary>
    /// <returns></returns>
    Task<(string host, int port)> ResolveAsync();
}

public class DefaultRpcServiceDiscovery : IRpcServiceDiscovery
{
    private readonly RpcClientOptions _options;

    public DefaultRpcServiceDiscovery(IOptions<RpcClientOptions> options)
    {
        this._options = options.Value;
    }

    public Task<(string host, int port)> ResolveAsync()
    {
        return Task.FromResult((this._options.Host, this._options.Port));
    }
}
