using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
// HttpClient is in the System.web.Http namespace.
using System.Net.Http;
// To use JsonSerializer, you need to add System.Text.Json. Depending on your project type and version, you may find it as assembly reference or Nuget package
// (right-click on the project --> (add --> Reference) or Manage NuGet packages).
using System.Text.Json;
// GeoCordinates is in the System.Device.Location namespace, coming from System.Device which is an assembly reference.
using System.Device.Location;
using System.Net;


namespace RestClient

{
    internal class Program
    {
        private static readonly HttpClient client = new HttpClient();



        static void Main(string[] args)
        {
            using (WebClient webClient = new WebClient()) ;

            string query, apiKey, url, response, address;
            client.DefaultRequestHeaders.Add("User-Agent", "RoutingServer");
            // 0 : Ask the user for his/her API key.
            //Console.WriteLine("Please enter your JCDecaux's API Key");
            //apiKey = Console.ReadLine();
            Console.WriteLine("Enter your address");
            address = Console.ReadLine();
            //address = "Paris";


            // 1.1: Retrieve all contracts.
            //query = "apiKey="+apiKey ;
            query = "q=\"" + address.Replace(' ', '+') + "&format=json&limit=1&addressdetails=1";
            //url = "https://api.jcdecaux.com/vls/v3/contracts";
            url = "https://nominatim.openstreetmap.org/search.php";
            response = JCDecauxAPICall(url, query).Result;
            Console.WriteLine(response);

            List<OSMAdress> adresses = JsonSerializer.Deserialize<List<OSMAdress>>(response);
            OSMAdress adress = adresses[0];
            Console.WriteLine(adress.display_name);
            Console.WriteLine("(" + adress.lat + ";" + adress.lon+")");
            Console.ReadLine();
            /*List<JCDContract> allContracts = JsonSerializer.Deserialize<List<JCDContract>>(response);

            // 1.2: Display all contracts.
            Console.WriteLine("Contracts:");
            foreach (JCDContract item in allContracts)
            {
                Console.WriteLine(item.name);
            }

            // 1.2: Ask the user to choose one.
            Console.WriteLine("Which contract are you interested in ?");
            string contract = Console.ReadLine();
            Console.WriteLine("Chosen contract: " + contract);

            // 2.1 Retrieve all stations
            url = "https://api.jcdecaux.com/vls/v3/stations";
            query = "contract=" + contract + "&apiKey=" + apiKey;
            response = JCDecauxAPICall(url, query).Result;
            List<JCDStation> allStations = JsonSerializer.Deserialize<List<JCDStation>>(response);

            // 2.2: Display all stations.
            Console.WriteLine("Stations:");
            foreach (JCDStation item in allStations)
            {
                Console.WriteLine(item.number + ": " + item.name);
            }

            // 2.2: Ask the user to choose one.
            Console.WriteLine("Which station are you interested in ?");
            int stationNumber = Int32.Parse(Console.ReadLine().Split(new[] { ':' })[0]);

            // 3.0: Retrieve the chosen station.
            JCDStation chosenStation = allStations[0];
            foreach (JCDStation item in allStations)
            {
                if (item.number == stationNumber) {
                    chosenStation = item;
                    break;
                }
            }
            Console.WriteLine("Chosen station: " + chosenStation.name);

            //3.1 : Compute the closest station.
            GeoCoordinate stationCoordinates = new GeoCoordinate(chosenStation.position.latitude, chosenStation.position.longitude);

            Double minDistance = -1;
            JCDStation closestStation = chosenStation;
            foreach (JCDStation item in allStations)
            {
                // Find the current station's position.
                GeoCoordinate candidatePos = new GeoCoordinate(item.position.latitude, item.position.longitude);
                // And compare its distance to the chosen one to see if it is closer than the current closest.
                Double distanceToCandidate = stationCoordinates.GetDistanceTo(candidatePos);

                if (distanceToCandidate != 0 && (minDistance == -1 || distanceToCandidate < minDistance))
                {
                    closestStation = item;
                    minDistance = distanceToCandidate;
                }
            }

            Console.WriteLine("Closest station: " + closestStation.name);
            Console.ReadLine();*/
        }
            

        // Task is the only possible return value on an async function. If you need to specify a type, you can use the <> notation, for instance Task<string>.
        static async Task<string> JCDecauxAPICall(string url, string query) 
        {
            //HttpResponseMessage response = await client.GetAsync(url + "?" + query);
            //Uri uri = new Uri("https://nominatim.openstreetmap.org/search.php?q=111+avenue+des+pugets&format=json&limit=1&addressdetails=1");
            Uri uri = new Uri(url + "?" + query);
            HttpResponseMessage response = await client.GetAsync(uri);
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadAsStringAsync();
        }
    }

    public class JCDContract
    {
        public string name { get; set; }
    }

    public class JCDStation
    {
        public int number { get; set; }
        public string name { get; set; }
        public Position position { get; set; }
    }

    public class Position { 
        public Double latitude { get; set; }
        public Double longitude { get; set; }
    }

    public class OSMAdress
    {
        public string lat { get; set; }
        public string lon { get; set; }  
        public string display_name { get; set; }
    }

    
}
