using System.ServiceModel;

namespace RelyingParty
{
    [ServiceContract]
    public interface IEchoService
    {
        [OperationContract(Name = "SendString")]
        string SendString(string message);
    }
}