using Common;
using Middling.Services;
using static AuthReally.Grpc.Greeter;

var app = new StartUp(args);

app.AddServices = builder =>
{
    builder.Services.AddGrpc();
    builder.Services.AddCertificateAuthentication();

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

    app.Log.LogCritical("WAAAAAAAAAAAAAANKER!");
};

app.ConfigureRequestPipeline = pipeline =>
{
    pipeline.MapGrpcService<MiddleService>();
    pipeline.MapGet("/", () => "Wanker");
};

return app.Run();
