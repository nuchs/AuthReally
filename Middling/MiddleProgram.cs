using Common;
using Middling.Services;
using System.Net.Security;
using System.Security.Authentication;
using System.Security.Cryptography.X509Certificates;
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
    builder.Services.AddGrpcClient<GreeterClient>("AuthReally", o =>
    {
        app.Log.LogInformation("Connecting to {}", builder.Configuration["ConnectionStrings:AuthReally"]);
        o.Address = new Uri(builder.Configuration["ConnectionStrings:AuthReally"]);
    }).ConfigurePrimaryHttpMessageHandler(() =>
    {
        var handler = new HttpClientHandler();
        handler.ClientCertificates.Add(new X509Certificate2(builder.Configuration["ServerCert"], builder.Configuration["ServerCertPassword"]));
        handler.ServerCertificateCustomValidationCallback = CheckServerCert;
        handler.SslProtocols = SslProtocols.Tls12;
        return handler;
    });
};

app.ConfigureRequestPipeline = pipeline =>
{
    pipeline.MapGrpcService<MiddleService>();
    pipeline.MapGet("/", () => "Wanker");
};

return app.Run();

bool CheckServerCert(HttpRequestMessage request, X509Certificate2? cert, X509Chain? chain, SslPolicyErrors errors)
{
    app.Log.LogInformation("Client checking server cert for {}, errors {}", cert.SubjectName.Name, errors);

    return true;
}
