using RoutingServerBike;
using System.Collections.Generic;
using System.ServiceModel.Description;
using System.ServiceModel;
using System;

namespace ServerBike2
{
    class Program
    {
        static void Main(string[] args)
        {
            //Create a URI to serve as the base address
            //Be careful to run Visual Studio as Admistrator or to allow VS to open new port netsh command. 

            Uri httpUrl = new Uri("http://localhost:8090/MyService/BikeService");

            //Create ServiceHost
            ServiceHost host = new ServiceHost(typeof(BikeService), httpUrl);

            // Multiple end points can be added to the Service using AddServiceEndpoint() method.
            // Host.Open() will run the service, so that it can be used by any client.

            // Example adding :
            // Uri tcpUrl = new Uri("net.tcp://localhost:8090/MyService/SimpleCalculator");
            // ServiceHost host = new ServiceHost(typeof(MyCalculatorService.SimpleCalculator), httpUrl, tcpUrl);

            //Add a service endpoint
            host.AddServiceEndpoint(typeof(IBikeService), new WSHttpBinding(), "");

            //Enable metadata exchange
            ServiceMetadataBehavior smb = new ServiceMetadataBehavior();
            smb.HttpGetEnabled = true;
            host.Description.Behaviors.Add(smb);

            //Start the Service
            host.Open();

            //Console.WriteLine("Service is host at " + DateTime.Now.ToString());

            BikeService service = new BikeService();
            //service.getContracts();


            Console.WriteLine("Lets try to find a station near latitude : 0.0 and longitude : 60.0");

            List<Station> stations = service.askStationsOfAContract("toulouse").Result;
            
            Console.WriteLine(service.findClosestStation(0.0, 60.0, stations));
            Console.WriteLine("Host is running... Press <Enter> key to stop");
            Console.ReadLine();
        }
    }
}
