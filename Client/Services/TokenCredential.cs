using Grpc.Core;
using Grpc.Core.Interceptors;

namespace Client.Services
{
    public class TokenCredential : Interceptor
    {
        private readonly string _token;

        public TokenCredential(string token)
        {
            _token = token;
        }
        public override Task<TResponse> UnaryServerHandler<TRequest, TResponse>(
            TRequest request,
            ServerCallContext context,
            UnaryServerMethod<TRequest, TResponse> continuation)
        {
            context.RequestHeaders.Add("authorization", $"Bearer {_token}");

            return continuation(request, context);
        }
    }
}
