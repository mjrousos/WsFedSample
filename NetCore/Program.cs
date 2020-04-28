using System;
using System.Diagnostics;
using System.IdentityModel.Tokens;
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
        static async Task Main()
        {
            using var echoClient = new EchoServiceClient(GetBinding(), GetEndpoint());

            echoClient.ClientCredentials.UserName.UserName = @"username";
            echoClient.ClientCredentials.UserName.Password = @"password";
            echoClient.ClientCredentials.ServiceCertificate.SslCertificateAuthentication = new X509ServiceCertificateAuthentication() 
            { 
                CertificateValidationMode = X509CertificateValidationMode.None,
                RevocationMode = X509RevocationMode.NoCheck
            };

            Console.WriteLine(await echoClient.SendStringAsync("TestABCDEF"));

            Console.WriteLine("Done");
            if (Debugger.IsAttached)
            {
                Console.WriteLine("Press any key to exit");
                Console.ReadKey();
            }
        }

        private static EndpointAddress GetEndpoint() => new EndpointAddress("https://127.0.0.1:443/IssuedTokenUsingTls");

        private static Binding GetBinding()
        {
            var issuerBinding = new WS2007HttpBinding(SecurityMode.TransportWithMessageCredential);
            issuerBinding.Security.Message.ClientCredentialType = MessageCredentialType.UserName;
            issuerBinding.Security.Message.EstablishSecurityContext = false;

            // TODO: Need CreateWsFederation2007HttpBinding helper
            var binding = new WsFederationHttpBinding(new WsTrustTokenParameters
            {
                IssuerBinding = issuerBinding,
                IssuerAddress = new EndpointAddress("https://issueraddress/adfs/services/trust/13/usernamemixed"),
                TokenType = "http://docs.oasis-open.org/wss/oasis-wss-saml-token-profile-1.1#SAMLV1.1",
                MessageSecurityVersion = MessageSecurityVersion.WSSecurity11WSTrust13WSSecureConversation13WSSecurityPolicy12BasicSecurityProfile10,

                // TODO : This shouldn't be necessary!
                KeyType = SecurityKeyType.SymmetricKey
            });

            // TODO: I noticed that binding.Security.Message.ClientCredentialType is 'Windows' instead of 'IssuedToken'. Seems wrong.
            binding.Security.Message.ClientCredentialType = MessageCredentialType.IssuedToken;
            binding.Security.Mode = SecurityMode.TransportWithMessageCredential;
            binding.Security.Message.EstablishSecurityContext = false;

            return binding;
        }
    }
}
