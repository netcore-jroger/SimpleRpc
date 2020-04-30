using ProtoBuf;
using SimpleRpc.Shared;
using SimpleRpc.Shared.ServiceAnnotations;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace InterfaceLib
{
    [RpcService]
    public interface IUserService : IRpcService
    {
        [RpcMethod]
        Task<UserDTO> GetUserBy(UserRequest request, CancellationToken token = default);

        [RpcMethod(MethodType = Grpc.Core.MethodType.ClientStreaming)]
        Task<UserDTO> TestClientStreaming(UserRequest request, CancellationToken token = default);
    }

    [ProtoContract]
    public class UserDTO
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
