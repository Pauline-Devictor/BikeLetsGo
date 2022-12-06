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


            host.AddServiceEndpoint(typeof(IBikeService), new BasicHttpBinding(), "");


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
