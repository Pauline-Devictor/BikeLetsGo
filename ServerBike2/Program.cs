using RoutingServerBike;
using System.ServiceModel.Description;
using System.ServiceModel;
using System;

namespace ServerBike2
{
    class Program
    {
        static void Main(string[] args)
        {
            //Doesn't work ? -> Is it launched as Admin ?
            Uri httpUrl = new Uri("http://localhost:8090/MyService/BikeService");

            ServiceHost host = new ServiceHost(typeof(BikeService), httpUrl);

            // Multiple end points can be added to the Service using AddServiceEndpoint() method.

            // Example adding :
            //
            //Uri tcpUrl = new Uri("net.tcp://localhost:8090/MyService/SimpleCalculator");
            // ServiceHost host = new ServiceHost(typeof(BikeService), httpUrl, tcpUrl);
            BasicHttpBinding binding = new BasicHttpBinding();

            binding.MaxReceivedMessageSize = 1000000;
            host.AddServiceEndpoint(typeof(IBikeService), binding, "");
            host.IncrementManualFlowControlLimit(1000000);


            //Enable metadata exchange
            ServiceMetadataBehavior smb = new ServiceMetadataBehavior();
            smb.HttpGetEnabled = true;
            host.Description.Behaviors.Add(smb);

            host.Open();

            Console.WriteLine("Service is host at " + DateTime.Now.ToString());
            Console.WriteLine("Host is running... Press <Enter> key to stop");
            Console.ReadLine();
        }
    }
}
