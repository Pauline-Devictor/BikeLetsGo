using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using System.Device.Location;
using System.Text.Json;
using System.Diagnostics.Contracts;

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
            return findClosestStation(latitude,longitude,stations);
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

        public string findClosestStation(double latitude, double longitude,List<Station> stations)
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


        public void testOSM()
        {
            
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
