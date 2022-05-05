using AuthReally.Grpc;
using Grpc.Core;
using Middle.Grpc;
using static AuthReally.Grpc.Greeter;

namespace Middling.Services;
public class MiddleService : Middler.MiddlerBase
{
    private readonly ILogger<MiddleService> _logger;
    private readonly GreeterClient client;

    public MiddleService(GreeterClient client, ILogger<MiddleService> logger)
    {
        _logger = logger;
        this.client = client;
    }

    public override async Task<MiddleReply> SayHello(MiddleRequest request, ServerCallContext context)
    {
        var id = WhoAmI(context);
        try
        {
            _logger.LogInformation("Proxying greeting request from {name}", id);

            var result = await client.SayHelloAsync(new HelloRequest()
            {
                Name = id
            });

            return new MiddleReply
            {
                Message = "Proxied :" + result.Message
            };

        }
        catch (Exception e)
        {
            _logger.LogWarning($"Failed to handle request from {id}: {e}");
            return new MiddleReply { Message = "nay joy" };
        }    
    }

    private string WhoAmI(ServerCallContext context)
            => context.AuthContext.PeerIdentity.FirstOrDefault()?.Value ?? "No-one";
}
