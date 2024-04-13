namespace SimpleRpc.Client;

public class RpcClientOptions
{
    /// <summary>
    /// 获取或设置 RPC 服务的主机。
    /// </summary>
    public string Host { get; set; }

    /// <summary>
    /// 获取或设置 RPC 服务的端口
    /// </summary>
    public int Port { get; set; }
    
    /// <summary>
    /// 获取或设置RPC服务是否启用了服务发现。
    /// </summary>
    public bool EnableServiceDiscovery { get; set; }
}
