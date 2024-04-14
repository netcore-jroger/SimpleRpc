// Copyright (c) JRoger. All Rights Reserved.

using Grpc.Core;
using SimpleRpc.Shared.Serializers;
using System.Runtime.CompilerServices;

[assembly:InternalsVisibleTo("SimpleRpc.Server")]
[assembly: InternalsVisibleTo("SimpleRpc.Client")]

namespace SimpleRpc.Shared.Internal
{
    internal static class MethodDefinitionGenerator
    {
        public static Method<TRequest, TResponse> CreateMethodDefinition<TRequest, TResponse>(MethodType methodType, string serviceName, string methodName, ISerializer serializer)
            where TRequest : class
            where TResponse : class
        {
            var method = new Method<TRequest, TResponse>(
                type: methodType,
                serviceName: serviceName,
                name: methodName,
                requestMarshaller: Marshallers.Create(
                    serializer: serializer.ToBytes<TRequest>,
                    deserializer: serializer.FromBytes<TRequest>
                ),
                responseMarshaller: Marshallers.Create(
                    serializer: serializer.ToBytes<TResponse>,
                    deserializer: serializer.FromBytes<TResponse>
                )
            );

            return method;
        }
    }
}
