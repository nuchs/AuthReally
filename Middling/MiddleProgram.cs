using Common;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Middling.Services;
using System.Net;
using static AuthReally.Grpc.Greeter;

var app = new StartUp(args);

app.AddServices = builder =>
{
    builder.Services.AddGrpc();
    builder.Services.AddCertificateAuthentication();

    builder.Services.AddHealthChecks();
    builder.Services.Configure<KestrelServerOptions>(opt =>
    {
        opt.Listen(IPAddress.Any, 33333);
    });

    builder.Services.AddMtlsEndpoint(
        int.Parse(builder.Configuration["ServerPort"]),
        builder.Configuration["ServerCert"],
        builder.Configuration["ServerCertPassword"],
        app.Log);

    builder.Services.AddMtlsGrpcClient<GreeterClient>(
        builder.Configuration["ConnectionStrings:AuthReally"],
        builder.Configuration["ClientCert"],
        builder.Configuration["ClientCertPassword"],
        app.Log);
};

app.ConfigureRequestPipeline = pipeline =>
{
    pipeline.MapGrpcService<MiddleService>().RequireHost($"*:{pipeline.Configuration["ServerPort"]}");
    pipeline.MapHealthChecks("/health").RequireHost("*:33333");
    pipeline.MapGet("/", () => "Wankered");
};

return app.Run();
