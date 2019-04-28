using System;

namespace SimpleRpc.Shared.ServiceAnnotations
{
    [AttributeUsage(AttributeTargets.Method, Inherited = true)]
    public class RpcMethodAttribute : Attribute
    {
        public string Name { get; set; }

        public RpcMethodAttribute(string name = "")
        {
            this.Name = name;
        }
    }
}
