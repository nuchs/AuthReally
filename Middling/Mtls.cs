using Microsoft.AspNetCore.Authentication.Certificate;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.AspNetCore.Server.Kestrel.Https;
using System.Net;
using System.Net.Security;
using System.Security.Authentication;
using System.Security.Cryptography.X509Certificates;

namespace Common;

public static class Mtls
{
    public static IServiceCollection AddCertificateAuthentication(this IServiceCollection target)
    {
        target.AddAuthentication(CertificateAuthenticationDefaults.AuthenticationScheme)
                   .AddCertificate(opt =>
                   {
                       opt.RevocationMode = X509RevocationMode.NoCheck;
                       opt.AllowedCertificateTypes = CertificateTypes.Chained;
                       opt.ValidateCertificateUse = true;
                       opt.ValidateValidityPeriod = true;
                   });

        return target;
    }

    public static IServiceCollection AddMtlsEndpoint(this IServiceCollection target, int port, string serverCert, string certPassword, ILogger? log = null)
    {
        target.Configure<KestrelServerOptions>(opt =>
        {
            opt.Listen(IPAddress.Any, port, listenOpts =>
            {
                listenOpts.Protocols = HttpProtocols.Http2;
                listenOpts.UseHttps(serverCert, certPassword, httpsOpt =>
                {
                    httpsOpt.SslProtocols = SslProtocols.Tls12;
                    httpsOpt.ClientCertificateMode = ClientCertificateMode.RequireCertificate;
                    httpsOpt.ClientCertificateValidation = MakeClientCertificateValidator(log);
                });
            });
        });

        return target;
    }

    public static IServiceCollection AddMtlsGrpcClient<T>(this IServiceCollection target, string connectionString, string clientCert, string certPassword, ILogger? log = null)
        where T: class
    {
        target.AddGrpcClient<T>("AuthReally", o =>
        {
            log?.LogInformation("Creating {} client connecting to {}", typeof(T), connectionString);
            o.Address = new Uri(connectionString);
        })
        .ConfigurePrimaryHttpMessageHandler(() =>
        {
            var handler = new HttpClientHandler();
            handler.ClientCertificates.Add(new X509Certificate2(clientCert, certPassword));
            handler.ServerCertificateCustomValidationCallback = MakeServerCertificateValidator(log);
            handler.SslProtocols = SslProtocols.Tls12;
            return handler;
        });

        return target;
    }

    private static Func<HttpRequestMessage, X509Certificate?, X509Chain?, SslPolicyErrors, bool> MakeServerCertificateValidator(ILogger? log)
    {
        return (request, cert, chain, errors) =>
        {
            if (cert == null)
            {
                log?.LogWarning("Server did not provide certificate when connecting to {}", request.RequestUri);

                return false;
            }

            log?.LogInformation("Client checking server cert for {}", cert.Subject);

            if (errors != SslPolicyErrors.None)
            {
                log?.LogError("Client is rejecting connection to {} : {}", cert.Subject, errors);
                return false;
            }

            return true;
        };
    }

    private static Func<X509Certificate2, X509Chain?, SslPolicyErrors, bool> MakeClientCertificateValidator(ILogger? log)
    {
        return (cert, chain, errors) =>
        {
            log?.LogInformation("Kestral checking client cert for {}", cert.SubjectName.Name);

            if (errors != SslPolicyErrors.None)
            {
                log?.LogError("Kestral is rejecting connection from {} : {}", cert.SubjectName.Name, errors);
                return false;
            }

            return true;
        };
    }
}
