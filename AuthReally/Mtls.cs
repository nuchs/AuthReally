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
                    httpsOpt.ClientCertificateValidation = MakeCertificateValidator(log);
                });
            });
        });

        return target;
    }

    private static Func<X509Certificate2, X509Chain?, SslPolicyErrors, bool> MakeCertificateValidator(ILogger? log)
    {
        return (cert, chain, errors) =>
        {
            log?.LogInformation("Kestral checking cert for {}", cert.SubjectName.Name);

            if (errors != SslPolicyErrors.None)
            {
                log?.LogError("Kestral is rejecting connection from {} : {}", cert.SubjectName.Name, errors);
                return false;
            }

            return true;
        };
    }
}
