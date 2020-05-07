using System;
using System.Threading;
using System.Threading.Tasks;
using Grpc.Core;
using ProtoBuf;
using SimpleRpc.Shared;
using SimpleRpc.Shared.ServiceAnnotations;

namespace InterfaceLib
{
    [RpcService("greet.Greeter")]
    public interface IUserService : IRpcService
    {
        [RpcMethod]
        Task<UserDto> GetUserBy(UserRequest request, CancellationToken token = default);

        [RpcMethod(MethodType = MethodType.ClientStreaming, RequestDataType = typeof(UserDto))]
        Task<UserDto> TestClientStreaming(CancellationToken token = default);
    }

    [ProtoContract]
    public class UserDto
    {
        [ProtoMember(1)]
        public int Id { get; set; }

        [ProtoMember(2)]
        public string Name { get; set; }

        [ProtoMember(3)]
        public DateTime CreateDate { get; set; }
    }

    [ProtoContract]
    public class UserRequest
    {
        [ProtoMember(1)]
        public int Id { get; set; }

        [ProtoMember(2)]
        public string Keyword { get; set; }
    }
}
