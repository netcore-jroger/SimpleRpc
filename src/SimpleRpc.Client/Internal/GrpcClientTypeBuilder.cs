using System;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using SimpleRpc.Shared;
using SimpleRpc.Shared.ServiceAnnotations;

namespace SimpleRpc.Client.Internal
{
    internal class GrpcClientTypeBuilder
    {
        private static readonly Type _clientBaseType = typeof(GrpcClientBase);
        private static readonly ConstructorInfo _ctorToCall = _clientBaseType.GetConstructor(BindingFlags.NonPublic | BindingFlags.Instance, null, new[] { typeof(IRpcChannel) }, null);
        private static readonly MethodInfo _methodToCall = _clientBaseType.GetMethod("CallUnaryMethodAsync", BindingFlags.Instance | BindingFlags.NonPublic);

        public TypeInfo Create<TService>()
            where TService : class, IRpcService
        {
            var assemblyName = $"GrpcClientProxy_{Guid.NewGuid():N}";
            var assemblyBuilder = AssemblyBuilder.DefineDynamicAssembly(new AssemblyName(assemblyName), AssemblyBuilderAccess.Run);
            var moduleBuilder = assemblyBuilder.DefineDynamicModule(assemblyName);

            var serviceType = typeof(TService);
            var typeBuilder = moduleBuilder.DefineType(serviceType.Name + "GrpcClientProxy", TypeAttributes.Public, _clientBaseType);

            typeBuilder.AddInterfaceImplementation(serviceType);
            this.AddConstructor(typeBuilder, serviceType);
            this.AddMethods(typeBuilder, serviceType);

            return typeBuilder.CreateTypeInfo();
        }

        private void AddConstructor(TypeBuilder typeBuilder, Type serviceType)
        {
            var ctorBuilder = typeBuilder.DefineConstructor(
                MethodAttributes.Public,
                CallingConventions.Standard,
                new[] { typeof(IRpcChannel) }
            );

            var il = ctorBuilder.GetILGenerator();
            il.Emit(OpCodes.Ldarg_0);
            il.Emit(OpCodes.Ldarg_1);
            il.Emit(OpCodes.Call, _ctorToCall);
            il.Emit(OpCodes.Ret);
        }

        private void AddMethods(TypeBuilder typeBuilder, Type serviceType)
        {
            foreach (var method in serviceType.GetMethods().Where(_ => _.GetCustomAttribute(typeof(RpcMethodAttribute), true) != null))
            {
                this.AddMethod(typeBuilder, method);
            }
        }

        private void AddMethod(TypeBuilder typeBuilder, MethodInfo method)
        {
            var serviceName = ((RpcServiceAttribute)method.DeclaringType.GetCustomAttribute(typeof(RpcServiceAttribute)))?.Name;
            if (string.IsNullOrWhiteSpace(serviceName)) serviceName = method.DeclaringType.Name;

            var methodName = ((RpcMethodAttribute)method.GetCustomAttribute(typeof(RpcMethodAttribute)))?.Name;
            if (string.IsNullOrWhiteSpace(methodName)) methodName = method.Name;

            var args = method.GetParameters();
            var methodBuilder = typeBuilder.DefineMethod(
                method.Name,
                MethodAttributes.Public | MethodAttributes.Virtual,
                method.ReturnType,
                (from arg in args select arg.ParameterType).ToArray()
            );
            var il = methodBuilder.GetILGenerator();
            il.Emit(OpCodes.Ldarg_0);
            il.Emit(OpCodes.Ldarg_1);
            il.Emit(OpCodes.Ldstr, serviceName);
            il.Emit(OpCodes.Ldstr, methodName);
            il.Emit(OpCodes.Ldarg_2);

            il.Emit(
                OpCodes.Call,
                _methodToCall.MakeGenericMethod(new [] {
                    method.GetParameters()[0].ParameterType,
                    method.ReturnType.GetGenericArguments()[0]
                })
            );

            il.Emit(OpCodes.Ret);

            typeBuilder.DefineMethodOverride(methodBuilder, method);
        }
    }
}
