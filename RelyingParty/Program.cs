using Serilog;
using System;
using System.IO;
using System.Security.Cryptography.X509Certificates;
using System.ServiceModel;
using System.ServiceModel.Security;

namespace RelyingParty
{
    class Program
    {
        static void Main()
        {
            ConfigureLogging();

            using (var host = new ServiceHost(typeof(Calculator)))
            {
                // For demo purposes, just load the key from disk so that no one needs to install an untrustworthy self-signed cert
                var certPath = Path.Combine(Path.GetDirectoryName(typeof(Program).Assembly.Location), "RelyingParty.pfx");
                host.Credentials.ServiceCertificate.Certificate = new X509Certificate2(certPath, "[CertPassword]");
                host.Credentials.ClientCertificate.Authentication.CertificateValidationMode = X509CertificateValidationMode.None;
                host.Open();
                Log.Information("Calculator Service listening");

                Console.WriteLine("Press enter to exit");
                Console.ReadLine();
                Log.Information("Shutting down...");

                host.Close();
            }
        }

        private static void ConfigureLogging()
        {
            Log.Logger = new LoggerConfiguration()
                .WriteTo.Console()
                .CreateLogger();

            Log.Information("Logging initialized");
        }
    }
}
