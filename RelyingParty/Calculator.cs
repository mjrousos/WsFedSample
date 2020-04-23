using System.ServiceModel;

namespace RelyingParty
{
    [ServiceBehavior(ConcurrencyMode = ConcurrencyMode.Multiple)]
    class Calculator : ICalculator
    {
        public double Add(double x, double y) => x + y;

        public double Divide(double x, double y) => x / y;

        public double Multiply(double x, double y) => x * y;

        public double Subtract(double x, double y) => x - y;
    }
}
