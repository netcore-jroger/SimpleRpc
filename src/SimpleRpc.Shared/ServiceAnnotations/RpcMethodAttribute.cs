using System;
using Grpc.Core;

namespace SimpleRpc.Shared.ServiceAnnotations
{
    [AttributeUsage(AttributeTargets.Method, Inherited = true)]
    public class RpcMethodAttribute : Attribute
    {
        public string Name { get; set; }

        public MethodType MethodType { get; set; }

        public Type RequestDataType { get; set; }

        public Type ResponseDataType { get; set; }

        public RpcMethodAttribute(string name = "")
        {
            this.Name = name;
            this.MethodType = MethodType.Unary;
        }
    }
}
