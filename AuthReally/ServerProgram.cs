using AuthReally.Services;
using Microsoft.AspNetCore.Authentication.Certificate;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.AspNetCore.Server.Kestrel.Https;
using System.Net;
using System.Net.Security;
using System.Security.Authentication;
using System.Security.Cryptography.X509Certificates;

var log = CreateStartupLogger();
log.LogInformation("Hello!");
var builder = WebApplication.CreateBuilder(args);

builder.Services.AddGrpc();
builder.Services.AddAuthentication(CertificateAuthenticationDefaults.AuthenticationScheme)
    .AddCertificate(opt =>
    {
        opt.ChainTrustValidationMode = X509ChainTrustMode.CustomRootTrust; // This needs to be set otherwise options.CustomTrustStore is ignored
        opt.RevocationMode = X509RevocationMode.NoCheck;
        opt.AllowedCertificateTypes = CertificateTypes.Chained; // Self signed certs are auto rejected
        opt.ValidateCertificateUse = true; // Checks that it has the correct extensions for a client cert
        opt.ValidateValidityPeriod = true;
    });

builder.Services.Configure<KestrelServerOptions>(opt =>
{
    opt.Listen(IPAddress.Any, 443, listenOpts =>
    {
        listenOpts.Protocols = HttpProtocols.Http2;
        listenOpts.UseHttps("certs/authreally.localhost.pfx", "pfx_password", httpsOpt =>
        {
            httpsOpt.SslProtocols = SslProtocols.Tls12;
            httpsOpt.ClientCertificateMode = ClientCertificateMode.RequireCertificate;
            httpsOpt.ClientCertificateValidation = CheckClientCert;
        });
    });
});


log.LogInformation("App configured");

var app = builder.Build();
app.MapGrpcService<GreeterService>();
app.MapGet("/", () => "Wank");
log.LogInformation("Pipeline configured");
app.Run();
log.LogInformation("Bye");

ILogger<Program> CreateStartupLogger()
{
    using var loggerFactory = LoggerFactory.Create(
        builder => builder.AddSimpleConsole());

    return loggerFactory.CreateLogger<Program>();
}


bool CheckClientCert(X509Certificate2 cert, X509Chain? chain, SslPolicyErrors errors)
{
    log.LogInformation("Kestral checking client cert");
    log.LogInformation($"Subject = {cert.SubjectName.Name}");
    log.LogInformation($"Issuer = {cert.IssuerName.Name}");
    log.LogInformation($"From = {cert.NotBefore}");
    log.LogInformation($"To = {cert.NotAfter}");
    log.LogInformation($"Serial = {cert.SerialNumber}");
    log.LogInformation($"Errors = {errors}");
    log.LogInformation("Chain info");
    foreach (var state in chain.ChainStatus)
    {
        log.LogInformation($"Info {state.StatusInformation}, state {state.Status}");
    }

    return true;
}
