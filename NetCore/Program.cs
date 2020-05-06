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
            // Cache the WCF client so that its ChannelFactory is reused
            // This sort of caching (reusing channel factory) happens automatically
            // in NetFx when using config-based clients. In NetCore, though, users
            // will need to manage this themselves to keep good performance.
            if (EchoServiceClient == null)
            {
                EchoServiceClient = new EchoServiceClient(GetBinding(), Endpoint);
                EchoServiceClient.ClientCredentials.UserName.UserName = @"username";
                EchoServiceClient.ClientCredentials.UserName.Password = @"password";
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

            var binding = new WsFederationHttpBinding(new WsTrustTokenParameters
            {
                IssuerBinding = issuerBinding,
                IssuerAddress = new EndpointAddress("https://issueraddress/adfs/services/trust/13/usernamemixed"),

                // These shouldn't need set once there are helper methods for creating
                // WsFederationHttpBinding and Ws2007FederationHttpBinding instances.
                TokenType = "http://docs.oasis-open.org/wss/oasis-wss-saml-token-profile-1.1#SAMLV1.1",
                MessageSecurityVersion = MessageSecurityVersion.WSSecurity11WSTrust13WSSecureConversation13WSSecurityPolicy12BasicSecurityProfile10,

                // These two wouldn't normally be set, but are used here to make
                // sure that token issuing is done on each iteration since that's 
                // one of the code paths I want to check performance for.
                CacheIssuedTokens = false,
                EstablishSecurityContext = false,
            });

            return binding;
        }
    }
}
