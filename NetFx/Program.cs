using System;
using System.Net;
using System.ServiceModel.Security;
using System.Threading.Tasks;

namespace NetFx
{
    class Program
    {
        static async Task Main()
        {
            ServicePointManager.ServerCertificateValidationCallback = (_, __, ___, ____) => true;

            using (var echoClient = new EchoServiceClient())
            {
                echoClient.ClientCredentials.UserName.UserName = @"username";
                echoClient.ClientCredentials.UserName.Password = @"password";
                echoClient.ClientCredentials.ServiceCertificate.Authentication.CertificateValidationMode = X509CertificateValidationMode.None;

                echoClient.Open();

                Console.WriteLine(await echoClient.SendStringAsync("Test12345"));
            }
        }
    }
}
