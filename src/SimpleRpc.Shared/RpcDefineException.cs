using System;

namespace SimpleRpc.Shared
{
    public class RpcDefineException : Exception
    {
        public RpcDefineException(string message) : base(message) { }
    }
}
