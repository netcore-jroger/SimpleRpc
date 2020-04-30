using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using SimpleRpc.Shared;
using SimpleRpc.Shared.Description;

namespace SimpleRpc.Server
{
    public interface IRpcServiceTypeFinder
    {
        List<Type> FindAllRpcServiceType();

        List<RpcServiceDescription> GetAllRpcServiceDescription();
    }

    public class DefaultRpcServiceTypeFinder : IRpcServiceTypeFinder
    {
        private static readonly Regex _ignoreNamespaceRegex = new Regex("^System.*|^Microsoft.*|^Grpc.*|^netstandard*|^Newtonsoft.*|^Autofac.*|^SimpleRpc.*", RegexOptions.IgnoreCase);

        public List<Type> FindAllRpcServiceType()
        {
            var typeOfRpc = typeof(IRpcService);
            var rpcTypes = AppDomain.CurrentDomain.GetAssemblies()
                .Where(a => !_ignoreNamespaceRegex.IsMatch(a.GetName().Name) && !a.IsDynamic)
                .SelectMany(a => a.GetExportedTypes())
                .Where(t => t.IsInterface && typeOfRpc.IsAssignableFrom(t))
                .ToList();

            return rpcTypes;
        }

        public List<RpcServiceDescription> GetAllRpcServiceDescription()
            => this.FindAllRpcServiceType().Select(_ => new RpcServiceDescription(_)).ToList();
    }
}
