using AuthReally;
using Grpc.Core;

namespace AuthReally.Services;
public class GreeterService : Greeter.GreeterBase
{
    private readonly ILogger<GreeterService> _logger;
    public GreeterService(ILogger<GreeterService> logger)
    {
        _logger = logger;
    }

    public override Task<HelloReply> SayHello(HelloRequest request, ServerCallContext context)
    {
        var id = WhoAmI(context);
        _logger.LogInformation("Recived request from {}", id);

        return Task.FromResult(new HelloReply
        {
            Message = "Hello " + request.Name + ", or should I say - " + id
        });
    }

    private string WhoAmI(ServerCallContext context)
          => context.AuthContext.PeerIdentity.FirstOrDefault()?.Value ?? "No-one";
}
