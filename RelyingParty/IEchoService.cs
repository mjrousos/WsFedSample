using System.ServiceModel;

namespace RelyingParty
{
    [ServiceContract]
    interface IEchoService
    {
        [OperationContract(Name = "SendString")]
        string SendString(string message);
    }
}