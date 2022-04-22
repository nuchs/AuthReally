using Grpc.Net.Client;
using System.Security.Cryptography.X509Certificates;
using AuthReally;
using System.Security.Authentication;
using System.Net.Security;

string serverAddress = "https://localhost:" + args[0];
const string clientCertPath = @"certs\authreally.client.pfx";

using var channel = CreateSecureChannel();

var client = new Greeter.GreeterClient(channel);

Console.WriteLine("Submitting request");
var reply = await client.SayHelloAsync(new HelloRequest { Name = "GreeterClient" });

Console.WriteLine("Greeting: " + reply.Message);

GrpcChannel CreateSecureChannel()
{
    var handler = new HttpClientHandler();
    handler.ClientCertificates.Add(new X509Certificate2(clientCertPath, "pfx_password"));
    handler.ServerCertificateCustomValidationCallback = CheckServerCert;
    handler.SslProtocols = SslProtocols.Tls12;

    return GrpcChannel.ForAddress(serverAddress, new GrpcChannelOptions { HttpHandler = handler });
}

bool CheckServerCert(HttpRequestMessage request, X509Certificate2? cert, X509Chain? chain, SslPolicyErrors errors)
{
    Console.WriteLine("Client checking server cert");
    Console.WriteLine($"Subject = {cert.SubjectName.Name}");
    Console.WriteLine($"Issuer = {cert.IssuerName.Name}");
    Console.WriteLine($"From = {cert.NotBefore}");
    Console.WriteLine($"To = {cert.NotAfter}");
    Console.WriteLine($"Serial = {cert.SerialNumber}");
    Console.WriteLine($"Errors = {errors}");
    Console.WriteLine("Chain info");
    foreach (var state in chain.ChainStatus)
    {
        Console.WriteLine($"Info {state.StatusInformation}, state {state.Status}");
    }

    return true;
}
