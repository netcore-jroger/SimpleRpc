using System;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Grpc.Core;

namespace SimpleRpc.Shared
{
    public sealed class ClientStreaming<TRequest, TResponse> : Awaiter<TResponse>
        where TRequest : class
        where TResponse : class
    {
        public ClientStreaming() { }

        public ClientStreaming(TResponse response)
        {
            this.Response = response;
        }

        public AsyncClientStreamingCall<TRequest, TResponse> AsyncClientStreamingCall { get; internal set; }

        internal void SetAsyncClientStreamingCall(AsyncClientStreamingCall<TRequest, TResponse> call) => this.AsyncClientStreamingCall = call;

        public async Task WriteAsync(TRequest request) => await this.AsyncClientStreamingCall.RequestStream.WriteAsync(request);

        public async Task CompleteAsync() => await this.AsyncClientStreamingCall.RequestStream.CompleteAsync();

        public override void OnCompleted(Action continuation)
        {
            continuation();
        }

        public async override Task<TResponse> GetResult()
        {
            var response = await this.AsyncClientStreamingCall;

            return response;
        }

        public TResponse Response { get; private set; }

        public override bool IsCompleted => this.AsyncClientStreamingCall.GetAwaiter().IsCompleted;
    }

    public abstract class Awaiter<TResult> : INotifyCompletion
    {
        public abstract bool IsCompleted { get; }

        public abstract void OnCompleted(Action continuation);

        public Awaiter<TResult> GetAwaiter() => this;

        public abstract Task<TResult> GetResult();
    }
}
