using AuthReally.Services;
using Common;

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
};

app.ConfigureRequestPipeline = pipeline =>
{
    pipeline.MapGrpcService<GreeterService>();
    pipeline.MapGet("/", () => "Wank");
};

return app.Run();
