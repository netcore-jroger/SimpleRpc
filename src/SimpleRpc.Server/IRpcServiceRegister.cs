// Copyright (c) JRoger. All Rights Reserved.

using System;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Net.Http.Headers;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace SimpleRpc.Server;

/// <summary>
/// 定义服务注册。
/// </summary>
public interface IRpcServiceRegister
{
    /// <summary>
    /// 向服务治理中心注册RPC服务。
    /// </summary>
    /// <returns></returns>
    Task RegisterAsync();

    /// <summary>
    /// 向服务治理中心反注册RPC服务。
    /// </summary>
    /// <returns></returns>
    Task UnregisterAsync();
}

internal class NoopRpcServiceRegister : IRpcServiceRegister
{
    public Task RegisterAsync() => Task.CompletedTask;

    public Task UnregisterAsync() => Task.CompletedTask;
}

//internal class ConsulRpcServiceRegister : IRpcServiceRegister
//{
//    private readonly RpcServerOptions _options;
//    private readonly ConsulServiceOptions _consulServiceOptions;
//    private readonly ILogger<ConsulRpcServiceRegister> _logger;
//    private readonly HttpClient _httpClient;
//    private readonly string _localIP;

//    public ConsulRpcServiceRegister(
//        IOptions<RpcServerOptions> options,
//        IOptions<ConsulServiceOptions> consulServiceOptions,
//        ILoggerFactory loggerFactory,
//        HttpClient httpClient)
//    {
//        this._options = options.Value;
//        this._consulServiceOptions = consulServiceOptions.Value;
//        this._logger = loggerFactory?.CreateLogger<ConsulRpcServiceRegister>() ?? throw new ArgumentException(nameof(loggerFactory));
//        this._httpClient = httpClient ?? new HttpClient();
//        this._localIP = NetworkHelper.GetIpV4Address();
//    }

//    public async Task RegisterAsync()
//    {
//        if (!this._consulServiceOptions.Enable || this._options == null || !this._options.EnableServiceDiscovery) return;

//        var checkId = $"RPC_{this._localIP}";
//        var serviceId = $"HIS_RpcService_{this._localIP}";
//        var registration = JsonConvert.SerializeObject(new {
//            ID = serviceId,
//            Name = serviceId,
//            Address = this._localIP,
//            this._options.Port,
//            EnableTagOverride = true,
//            Check = new {
//                ID = checkId,
//                Name = checkId,
//                ServiceID = serviceId,
//                DeregisterCriticalServiceAfter = "10m",
//                Notes = $"IvyBaby.RPC.Server {this._localIP}:{this._options.Port}/{RpcConfigInformation.RpcHealthCheckPath}",
//                GRPC = $"{this._localIP}:{this._options.Port}/{RpcConfigInformation.RpcHealthCheckPath}",
//                GRPCUseTLS = false,
//                Interval = "10s",
//                Timeout = "10s"
//            },
//            Tags = new [] {
//                "HIS_RPC_SERVICE"
//            }
//        });
//        var content = new StringContent(registration, Encoding.UTF8);
//        content.Headers.ContentType = new MediaTypeHeaderValue("application/json");

//        this._logger.LogInformation($"RPC 服务注册参数：{registration}");

//        var uri = $"{this._consulServiceOptions.Scheme}://{this._consulServiceOptions.Host}:{this._consulServiceOptions.Port}/v1/agent/service/register";
//        var responseMessage = await this._httpClient.PutAsync(uri, content);
//        if (!responseMessage.IsSuccessStatusCode)
//        {
//            this._logger.LogInformation("注册：Consul 注册中心不可用");
//            return;
//        }

//        this._logger.LogInformation($"Consul 注册 RPC 服务结果：{responseMessage.StatusCode}");
//    }

//    public async Task UnregisterAsync()
//    {
//        if (this._options == null || !this._options.EnableServiceDiscovery || !this._consulServiceOptions.Enable) return;

//        var serviceId = $"HIS_RpcService_{this._localIP}";
//        var deregisterContent = new StringContent(string.Empty, Encoding.UTF8);
//        deregisterContent.Headers.ContentType = new MediaTypeHeaderValue("application/json");

//        this._logger.LogInformation("RPC 服务反注册");

//        var uri = $"{this._consulServiceOptions.Scheme}://{this._consulServiceOptions.Host}:{this._consulServiceOptions.Port}/v1/agent/service/deregister/{serviceId}";
//        var deRegisterResponseMessage = await this._httpClient.PutAsync(uri, deregisterContent);
//        if (!deRegisterResponseMessage.IsSuccessStatusCode)
//        {
//            this._logger.LogInformation("反注册：Consul 注册中心不可用");
//            return;
//        }

//        this._logger.LogInformation($"Consul 反注册 RPC 服务结果：{deRegisterResponseMessage.StatusCode}");
//    }
//}
