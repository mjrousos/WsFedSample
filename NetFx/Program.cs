using System;
using System.Diagnostics;
using System.Net;
using System.ServiceModel.Security;
using System.Threading.Tasks;

namespace NetFx
{
    class Program
    {
        const int Iterations = 1000;

        static async Task Main()
        {
            ServicePointManager.ServerCertificateValidationCallback = (_, __, ___, ____) => true;

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

        static async Task CreateAndCallClient()
        {
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
