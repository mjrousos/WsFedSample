using System;
using System.Threading.Tasks;

namespace NetFx
{
    class Program
    {
        static async Task Main()
        {
            var calculatorClient = new CalculatorServiceClient();
            calculatorClient.ClientCredentials.UserName.UserName = "username";
            calculatorClient.ClientCredentials.UserName.Password = "password";

            Console.WriteLine($"11 * 5 = {await calculatorClient.MultiplyAsync(11, 5)}");
        }
    }
}
