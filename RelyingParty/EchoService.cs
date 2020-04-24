using Serilog;
using System.ServiceModel;
using System.Threading;

namespace RelyingParty
{
    [ServiceBehavior]
    class EchoService : IEchoService
    {
        static int numberOfRequests = 0;

        [OperationBehavior]
        public string SendString(string message)
        {
            var requestNum = Interlocked.Increment(ref numberOfRequests);
            Log.Information("Service received: '{Message}', requestNumber: {RequestNumber}.", message, requestNum);

            string outbound = string.Format($"{requestNum}: Service received '{message}'.");
            Log.Information("Service returning: '{Reply}'.", outbound);
            return outbound;
        }
    }
}