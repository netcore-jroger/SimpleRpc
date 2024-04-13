using System;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using Grpc.Core;
using SimpleRpc.Shared;
using SimpleRpc.Shared.Description;

namespace SimpleRpc.Client.Internal;

internal class GrpcClientTypeBuilder
{
    private static readonly Type _clientBaseType = typeof(GrpcClientBase);
    private static readonly ConstructorInfo _ctorToCall = _clientBaseType.GetConstructor(BindingFlags.NonPublic | BindingFlags.Instance, null, new[] { typeof(IRpcChannel) }, null);
    private static readonly MethodInfo _unaryMethodToCall = _clientBaseType.GetMethod(nameof(IRpcChannel.CallUnaryMethodAsync), BindingFlags.Instance | BindingFlags.NonPublic);
    private static readonly MethodInfo _clientStreamingMethodToCall = _clientBaseType.GetMethod(nameof(IRpcChannel.AsyncClientStreamingCall), BindingFlags.Instance | BindingFlags.NonPublic);

    public TypeInfo Create<TService>()
        where TService : class, IRpcService
    {
        var assemblyName = $"SimpleRpc.ClientProxy_{Guid.NewGuid():N}";
        var assemblyBuilder = AssemblyBuilder.DefineDynamicAssembly(new AssemblyName(assemblyName), AssemblyBuilderAccess.Run);
        var moduleBuilder = assemblyBuilder.DefineDynamicModule(assemblyName);

        var serviceType = typeof(TService);
        var typeBuilder = moduleBuilder.DefineType(serviceType.Name + "GrpcClientProxy", TypeAttributes.Public, _clientBaseType);

        typeBuilder.AddInterfaceImplementation(serviceType);
        this.AddConstructor(typeBuilder);
        this.AddMethods(typeBuilder, serviceType);

        return typeBuilder.CreateTypeInfo();
    }

    private void AddConstructor(TypeBuilder typeBuilder)
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
        var serviceDescription = new RpcServiceDescription(serviceType);
        
        foreach (var methodDescription in serviceDescription.RpcMethods)
        {
            switch (methodDescription.RpcMethodType)
            {
                case MethodType.Unary:
                    this.AddUnaryMethod(typeBuilder, serviceDescription, methodDescription);
                    break;

                case MethodType.ClientStreaming:
                    this.AddClientStreamingMethod(typeBuilder, serviceDescription, methodDescription);
                    break;

                default:
                    throw new NotSupportedException($"Not support MethodType: {methodDescription.RpcMethodType}");
            }
        }
    }

    private void AddUnaryMethod(TypeBuilder typeBuilder, RpcServiceDescription serviceDescription, RpcMethodDescription methodDescription)
    {
        var args = methodDescription.RpcMethod.GetParameters();
        var methodBuilder = typeBuilder.DefineMethod(
            methodDescription.RpcMethodName,
            MethodAttributes.Public | MethodAttributes.Virtual,
            methodDescription.RpcMethod.ReturnType,
            (from arg in args select arg.ParameterType).ToArray()
        );
        var il = methodBuilder.GetILGenerator();
        il.Emit(OpCodes.Ldarg_0);
        il.Emit(OpCodes.Ldarg_1);
        il.Emit(OpCodes.Ldstr, serviceDescription.RpcServiceName);
        il.Emit(OpCodes.Ldstr, methodDescription.RpcMethodName);
        il.Emit(OpCodes.Ldarg_2);

        il.Emit(
            OpCodes.Call,
            _unaryMethodToCall.MakeGenericMethod(new [] {
                methodDescription.RpcMethod.GetParameters()[0].ParameterType,
                methodDescription.RpcMethod.ReturnType.GetGenericArguments()[0]
            })
        );

        il.Emit(OpCodes.Ret);

        typeBuilder.DefineMethodOverride(methodBuilder, methodDescription.RpcMethod);
    }

    private void AddClientStreamingMethod(TypeBuilder typeBuilder, RpcServiceDescription serviceDescription, RpcMethodDescription methodDescription)
    {
        var args = methodDescription.RpcMethod.GetParameters();
        var methodBuilder = typeBuilder.DefineMethod(
            methodDescription.RpcMethodName,
            MethodAttributes.Public | MethodAttributes.Virtual,
            methodDescription.RpcMethod.ReturnType,
            (from arg in args select arg.ParameterType).ToArray()
        );
        var il = methodBuilder.GetILGenerator();
        il.Emit(OpCodes.Ldarg_0);
        il.Emit(OpCodes.Ldstr, serviceDescription.RpcServiceName);
        il.Emit(OpCodes.Ldstr, methodDescription.RpcMethodName);
        il.Emit(OpCodes.Ldarg_1);

        il.Emit(
            OpCodes.Call,
            _clientStreamingMethodToCall.MakeGenericMethod(new[] {
                methodDescription.RequestDataType,
                methodDescription.RpcMethod.ReturnType.GetGenericArguments()[0]
            })
        );

        il.Emit(OpCodes.Ret);

        typeBuilder.DefineMethodOverride(methodBuilder, methodDescription.RpcMethod);
    }
}
