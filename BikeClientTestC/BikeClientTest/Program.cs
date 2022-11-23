using BikeClientTest.ServiceReference1;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Collections.Specialized.BitVector32;

namespace BikeClientTest
{
    internal class Program
    {
        static void Main(string[] args)
        {
            BikeServiceClient bikeServiceClient = new BikeServiceClient();
            string contracts = bikeServiceClient.getContracts();
            Console.WriteLine(contracts);


            Console.WriteLine("Lets try to find a station near latitude : 0.0 and longitude : 60.0");
            string stations = bikeServiceClient.getStations("toulouse");
            Console.WriteLine("Stations in Toulouse :");
            Console.WriteLine(stations.ToString());
            Console.WriteLine("Lets try to find closest station to latitude : 0.0 and longitude : 60.0 and in toulouse" );
            Console.WriteLine(bikeServiceClient.getStationInGivenCityCloseToUs(0.0,60.0,"toulouse"));
            
            Console.ReadLine();

        }
    }
    public class Station
    {
        public string name { get; set; }
        public int number { get; set; }
        public Position position { get; set; }

    }

    public class Position
    {
        public double latitude { get; set; }
        public double longitude { get; set; }
    }
}
