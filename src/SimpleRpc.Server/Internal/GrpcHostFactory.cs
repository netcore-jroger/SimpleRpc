using System;
using System.Linq;
using Microsoft.Extensions.DependencyInjection;

namespace SimpleRpc.Server.Internal;

internal static class GrpcHostFactory
{
    public static IRpcHost Create(IServiceProvider serviceProvider)
    {
        var rpcServiceTypeFinder = serviceProvider.GetRequiredService<IRpcServiceTypeFinder>();
        var builder = serviceProvider.GetRequiredService<IRpcHostBuilder>();

        //var rpcServices = rpcServiceTypeFinder.FindAllRpcServiceType();
        
        //// var rpcServices = services.Where(sd => sd.ServiceType.GetCustomAttribute<GrpcServiceAttribute>().IsNotNull());
        //if (rpcServices.Any())
        //{
        //    foreach (var rpcService in rpcServices)
        //    {
        //        // builder.AddUnaryMethods(rpcService.ServiceType);
        //        builder.AddUnaryMethods(rpcService);

        //        // TODO: refactor RegisterRpcServices ?
        //        // TODO: define a class RpcServiceDescription ?
        //    }
        //}

        var rpcServiceDescriptions = rpcServiceTypeFinder.GetAllRpcServiceDescription();
        if (rpcServiceDescriptions.Any())
        {
            builder.RegisterRpcService(rpcServiceDescriptions);
        }

        return builder.Build();
    }
}
