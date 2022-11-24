using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using System.Device.Location;
using System.Text.Json;
using System.Diagnostics.Contracts;
using System.Runtime.Remoting.Messaging;

namespace RoutingServerBike
{
    public class BikeService : IBikeService
    {

        static readonly HttpClient client = new HttpClient();
        public string getContracts()
        {
            return askContracts().Result;
        }

        public string getStations(string contract)
        {
            return askStationsOfAContract(contract).Result.ToString();
        }

        public string getStation(string stationNumber)
        {
            return "ask for a station";
        }
        public string getStationInGivenCityCloseToUs(double latitude, double longitude, string city)
        {
            List<Station> stations = askStationsOfAContract(city).Result;
            return findClosestStation(latitude, longitude, stations);
        }

        public string getItinerary(string departure, string arrival)
        {
            string adresses = "Depart de : " + getOSMAdress(departure).display_name + " \nArrivée à " + getOSMAdress(arrival).display_name;
            return adresses;
        }


        public OSMAdress getOSMAdress(string address)
        {
            string query, apiKey, url, response;
            client.DefaultRequestHeaders.Add("User-Agent", "RoutingServer");
            query = "q=\"" + address.Replace(' ', '+') + "&format=json&limit=1&addressdetails=1";
            //url = "https://api.jcdecaux.com/vls/v3/contracts";
            url = "https://nominatim.openstreetmap.org/search.php";
            response = JCDecauxAPICall(url, query).Result;
            Console.WriteLine(response);

            List<OSMAdress> adresses = JsonSerializer.Deserialize<List<OSMAdress>>(response);
            if (adresses != null)
            {
               return adresses[0];
            }
            else { throw new Exception(); }
            
        }



        public async Task<string> askContracts()
        {
            HttpResponseMessage response = await client.GetAsync("https://api.jcdecaux.com/vls/v1/contracts?apiKey=07a2f22eaa786639241a704c091d22c6875b5809");
            response.EnsureSuccessStatusCode();
            string responseBody = await response.Content.ReadAsStringAsync();
            Console.WriteLine(responseBody);
            return responseBody;
        }

        public async Task<List<Station>> askStationsOfAContract(string contract)
        {
            HttpResponseMessage response = await client.GetAsync("https://api.jcdecaux.com/vls/v1/stations?apiKey=07a2f22eaa786639241a704c091d22c6875b5809&contract=" + contract);
            response.EnsureSuccessStatusCode();
            string responseBody = await response.Content.ReadAsStringAsync();
            List<Station> stations = JsonSerializer.Deserialize<List<Station>>(responseBody);
            return stations;
        }

        public string findClosestStation(double latitude, double longitude, List<Station> stations)
        {
            GeoCoordinate clientPosition = new GeoCoordinate(latitude, longitude);

            double savedDistance = -1;
            Station currentClosestStation = null;
            GeoCoordinate chosenStationGo = new GeoCoordinate(latitude, longitude);

            foreach (Station station in stations)
            {
                GeoCoordinate currentstationGo = new GeoCoordinate(station.position.latitude, station.position.longitude);
                double distance = clientPosition.GetDistanceTo(currentstationGo);
                if (distance != 0 && (savedDistance == -1 || distance < savedDistance))
                {
                    savedDistance = distance;
                    currentClosestStation = station;
                }
            }
            return currentClosestStation.name;
        }




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

    public class OSMAdress
    {
        public string lat { get; set; }
        public string lon { get; set; }
        public string display_name { get; set; }
    }


}
