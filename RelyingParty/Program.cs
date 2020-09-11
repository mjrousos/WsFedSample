using Serilog;
using System;
using System.IdentityModel.Tokens;
using System.IO;
using System.Net;
using System.Security.Cryptography.X509Certificates;
using System.ServiceModel;
using System.ServiceModel.Security;

namespace RelyingParty
{
    class Program
    {
        static void Main()
        {
            ServicePointManager.ServerCertificateValidationCallback = (_, __, ___, ____) => true;
            ConfigureLogging();

            using (var host = new ServiceHost(typeof(EchoService)))
            {
                // For demo purposes, just load the key from disk so that no one needs to install an untrustworthy self-signed cert
                var certPath = Path.Combine(Path.GetDirectoryName(typeof(Program).Assembly.Location), "RelyingParty.pfx");
                host.Credentials.ServiceCertificate.Certificate = new X509Certificate2(certPath, "RelyingParty");
                host.Credentials.ClientCertificate.Authentication.CertificateValidationMode = X509CertificateValidationMode.None;

                host.Credentials.UseIdentityConfiguration = true;
                host.Credentials.IdentityConfiguration.AudienceRestriction.AudienceMode = System.IdentityModel.Selectors.AudienceUriMode.Never;
                host.Credentials.IdentityConfiguration.CertificateValidationMode = X509CertificateValidationMode.None;
                host.Credentials.IdentityConfiguration.IssuerNameRegistry = new CustomIssuerNameRegistry("authority");

                host.Open();
                foreach (var endpoint in host.Description.Endpoints)
                {
                    Log.Information("Echo Service listening on " + endpoint.ListenUri);
                }

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

    class CustomIssuerNameRegistry : IssuerNameRegistry
    {
        string _issuer;
        public CustomIssuerNameRegistry(string issuer)
        {
            _issuer = issuer;
        }

        public override string GetIssuerName(SecurityToken securityToken)
        {
            return _issuer;
        }
    }
}
