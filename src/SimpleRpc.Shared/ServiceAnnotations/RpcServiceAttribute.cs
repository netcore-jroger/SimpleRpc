using System;

namespace SimpleRpc.Shared.ServiceAnnotations
{
    [AttributeUsage(AttributeTargets.Interface, Inherited = true)]
    public class RpcServiceAttribute : Attribute
    {
        public string Name { get; set; }

        public RpcServiceAttribute(string name = "")
        {
            this.Name = name;
        }
    }
}
