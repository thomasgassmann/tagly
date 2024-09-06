using System;
using System.Threading.Tasks;
using Grpc.Core;
using Grpc.Net.Client;
using Tagly.Grpc;

namespace Tagly.App;

public class GrpcPhotosClient(string url, string token, bool secure)
{
    public Photos.PhotosClient Client => new(CreateAuthenticatedChannel());

    public async Task<string> GetJwtAsync()
    {
        using var channel = GrpcChannel.ForAddress(url, new GrpcChannelOptions
        {
            Credentials = secure ? ChannelCredentials.SecureSsl : ChannelCredentials.Insecure
        });
        var client = new Auth.AuthClient(channel);
        var response = await client.LoginAsync(new AuthRequest
        {
            ImportToken = token
        });
        return response.JwtToken;
    }
    
    private GrpcChannel CreateAuthenticatedChannel()
    {
        var credentials = CallCredentials.FromInterceptor(async (context, metadata) =>
        {
            var jwt = await GetJwtAsync();
            if (!string.IsNullOrEmpty(jwt))
            {
                metadata.Add("Authorization", $"Bearer {jwt}"); 
            }
        });

        var channel = GrpcChannel.ForAddress(url, new GrpcChannelOptions
        {
            Credentials = ChannelCredentials.Create(secure ? ChannelCredentials.SecureSsl : ChannelCredentials.Insecure, credentials),
            UnsafeUseInsecureChannelCallCredentials = !secure,
            MaxSendMessageSize = int.MaxValue,
            MaxReceiveMessageSize = int.MaxValue
        });
        return channel;
    }
}