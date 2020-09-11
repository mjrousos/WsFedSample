using System;
using System.Diagnostics;
using System.Security.Cryptography.X509Certificates;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.ServiceModel.Federation;
using System.ServiceModel.Security;
using System.Text;
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
            issuerBinding.TextEncoding = Encoding.UTF8;

            var binding = new WsFederationHttpBinding(new WsTrustTokenParameters
            {
                IssuerBinding = issuerBinding,
                IssuerAddress = new EndpointAddress("https://issueraddress/adfs/services/trust/13/usernamemixed"),

                // This won't need set once there are helper methods for creating
                // WsFederationHttpBinding and Ws2007FederationHttpBinding instances.
                MessageSecurityVersion = MessageSecurityVersion.WSSecurity11WSTrust13WSSecureConversation13WSSecurityPolicy12BasicSecurityProfile10,

                // These two properties are typically set to true. If you are trying to compare performance between NetFx and NetCore,
                // though, then you will want to uncomment these lines to get a fair comparison. Since the NetFx sample in this solution
                // isn't caching the WCF client, it has to recreate the secure conversation on every iteration even with SCT enabled. 
                // If these are uncommented, the RelyingParty and NetFx samples will also need to disable security context for the binding 
                // since the client and RP need to match.
                // 
                // In most real-world scenarios leveraging SCT, leaving these properties set to true (which is the default) is preferable.
                // 
                // CacheIssuedTokens = false,
                // EstablishSecurityContext = false,
            });

            return binding;
        }
    }
}
