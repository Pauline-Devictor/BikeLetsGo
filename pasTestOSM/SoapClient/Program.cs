using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
// Once added as a service reference, the namespace in which you will find the SOAP Client constructor is the name of your service
// (if you don't remember, you can find it in Connected Services). I used Calculator but I think the default is ServiceReference1.
using SoapClient.Calculator;

namespace SoapClient
{
    internal class Program
    {
        static void Main(string[] args)
        {
            // The name of the client class can be found by double clicking the connected service.
            CalculatorSoapClient client = new CalculatorSoapClient();
            Console.WriteLine("Enter the left value of your calculation");
            int left = Int32.Parse(Console.ReadLine());

            Console.WriteLine("Which operator to use (+, -, *, or /)");
            string calculOperator = Console.ReadLine();

            Console.WriteLine("Enter the right operand of your calculation");
            int right = Int32.Parse(Console.ReadLine());

            double result = 0;
            switch (calculOperator) {
                case "+":
                    result = client.Add(left, right);
                    break;
                case "-":
                    result = client.Subtract(left, right);
                    break;
                case "*":
                    result = client.Multiply(left, right);
                    break;
                case "/":
                    result = client.Divide(left, right);
                    break;
            }
            Console.WriteLine(left + " " + calculOperator + " " + right + " = " + result);

            Console.ReadLine();
        }
    }
}
