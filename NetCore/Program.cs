using System;
using System.Diagnostics;
using System.Security.Cryptography.X509Certificates;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.ServiceModel.Federation;
using System.ServiceModel.Security;
using System.Threading.Tasks;

namespace NetCore
{
    class Program
    {
        const int Iterations = 1000;
        static EndpointAddress Endpoint = new EndpointAddress("https://127.0.0.1:443/IssuedTokenUsingTls");
        static EchoServiceClient EchoServiceClient;

        static async Task Main()
        {
            Console.WriteLine("Warming up...");
            for (var i = 0; i < 10; i++)
            {
                await CreateAndCallClient();
            }

            Console.WriteLine("Testing...");
            var stopwatch = new Stopwatch();
            stopwatch.Start();
            for (var i = 0; i < Iterations; i++)
            {
                await CreateAndCallClient();
            }

            Console.WriteLine();
            Console.WriteLine($"{stopwatch.ElapsedMilliseconds} MS elapsed for {Iterations} iterations");
            Console.WriteLine();

            Console.WriteLine("Done");
            if (Debugger.IsAttached)
            {
                Console.WriteLine("Press any key to exit");
                Console.ReadKey();
            }
        }

        private static async Task CreateAndCallClient()
        {
            // Cache the WCF client so that its ChannelFactory is reused.
            // This sort of caching (reusing channel factory) happens automatically
            // in NetFx when using config-based clients. In NetCore, though, users
            // will need to manage this themselves to keep good performance.
            if (EchoServiceClient == null)
            {
                EchoServiceClient = new EchoServiceClient(GetBinding(), Endpoint);
                EchoServiceClient.ClientCredentials.UserName.UserName = @"username";
                EchoServiceClient.ClientCredentials.UserName.Password = @"password";

                // Don't validate certificates since they're just for test purposes
                EchoServiceClient.ClientCredentials.ServiceCertificate.SslCertificateAuthentication = new X509ServiceCertificateAuthentication()
                {
                    CertificateValidationMode = X509CertificateValidationMode.None,
                    RevocationMode = X509RevocationMode.NoCheck
                };
            }

            Console.WriteLine(await EchoServiceClient.SendStringAsync("TestABCDEF"));
        }

        private static Binding GetBinding()
        {
            var issuerBinding = new WS2007HttpBinding(SecurityMode.TransportWithMessageCredential);
            issuerBinding.Security.Message.ClientCredentialType = MessageCredentialType.UserName;
            issuerBinding.Security.Message.EstablishSecurityContext = false;

            var endpointAddress = new EndpointAddress("https://issueraddress/adfs/services/trust/13/usernamemixed");

            var tokenParameters = WSTrustTokenParameters.CreateWS2007FederationTokenParameters(issuerBinding, endpointAddress);

            // This works around https://github.com/dotnet/wcf/issues/4425 until an updated
            // System.ServiceModel.Federation package is published.
            tokenParameters.KeyType = System.IdentityModel.Tokens.SecurityKeyType.SymmetricKey;
            
            return new WSFederationHttpBinding(tokenParameters);
        }
    }
}
