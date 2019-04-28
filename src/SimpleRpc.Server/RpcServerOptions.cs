namespace SimpleRpc.Server
{
    public class RpcServerOptions
    {
        /// <summary>
        /// 获取或设置 RPC 服务的端口号。
        /// </summary>
        public int Port { get; set; }

        /// <summary>
        /// 获取或设置是否启用服务发现。
        /// </summary>
        public bool EnableServiceDiscovery { get; set; }
    }
}
