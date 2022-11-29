using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using System.Device.Location;
using System.Text.Json;

namespace RoutingServerBike
{
    public class BikeService : IBikeService
    {

        static readonly HttpClient client = new HttpClient();
  

        //TODO, permet de donner l'itinéraire au client
        public string getItinerary(string departure, string arrival)
        {
            OSMAdress depart;
            OSMAdress arrivee;
            try
            {
                depart = getOSMAdress(departure);
                arrivee = getOSMAdress(arrival);
            }
            catch (Exception ex)
            {
                return "Addresses not found, please try again";
            }
            //on peut optimiser en recherchant un contrat avec le même nom que la ville ou village du départ ou plus proche
            //Puis en cherchant une ville du même contrat la plus proche de l'arrivée

            string closestContractToDeparture = findClosestContract(depart);
            string closestContractToArrival = findClosestContract(arrivee);

            string arret1 = findClosestStation(depart , askStationsOfAContract(closestContractToDeparture).Result);

            string arret2 = findClosestStation(arrivee, askStationsOfAContract(closestContractToArrival).Result);


            
            string adresses = "Depart de : " + depart.display_name + " \nPrendre vélo à " + arret1 + "\nDeposer vélo à " + arret2 + " \nArrivée à " + arrivee.display_name + " ";
            return adresses;
        }


        public OSMAdress getOSMAdress(string address)
        {
            string query, apiKey, url, response;
            client.DefaultRequestHeaders.Add("User-Agent", "RoutingServer");
            query = "q=\"" + address.Replace(' ', '+') + "&format=jsonv2&limit=1&addressdetails=1";
            //url = "https://api.jcdecaux.com/vls/v3/contracts";
            url = "https://nominatim.openstreetmap.org/search.php";
            response = JCDecauxAPICall(url, query).Result;
            List<OSMAdress> adresses = JsonSerializer.Deserialize<List<OSMAdress>>(response);
            
            if (adresses != null)
            {
               return adresses[0];
            }
            else { throw new Exception(); }
            
        }


        public async Task<List<JCDContract>> askContracts()
        {
            HttpResponseMessage response = await client.GetAsync("https://api.jcdecaux.com/vls/v1/contracts?apiKey=07a2f22eaa786639241a704c091d22c6875b5809");
            response.EnsureSuccessStatusCode();
            string responseBody = await response.Content.ReadAsStringAsync();
            List<JCDContract> contracts = JsonSerializer.Deserialize<List<JCDContract>>(responseBody);

            contracts = cleanContractList(contracts);
            return contracts;
        }

        public async Task<List<Station>> askStationsOfAContract(string contract)
        {
            HttpResponseMessage response = await client.GetAsync("https://api.jcdecaux.com/vls/v1/stations?apiKey=07a2f22eaa786639241a704c091d22c6875b5809&contract=" + contract);
            response.EnsureSuccessStatusCode();
            string responseBody = await response.Content.ReadAsStringAsync();
            List<Station> stations = JsonSerializer.Deserialize<List<Station>>(responseBody);
            return stations;
        }

        public string findClosestStation(OSMAdress address, List<Station> stations)
        {
            GeoCoordinate clientPosition = new GeoCoordinate(changeToDouble(address.lat), changeToDouble(address.lon));

            double savedDistance = -1;
            Station currentClosestStation = null;

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


        
        //Retourne la ville la plus proche des coordonnées données
        public string findClosestContract(OSMAdress position)
        {
            double positionLat = changeToDouble(position.lat);
            double positionLon = changeToDouble(position.lon);
            GeoCoordinate coordIndication = new GeoCoordinate(positionLat, positionLon);
            List<JCDContract> contracts = askContracts().Result;
   
            double savedDistance = -1;
            String currentClosestContract = null;
            GeoCoordinate currentPosition = new GeoCoordinate(changeToDouble(position.lat), changeToDouble(position.lon));

            foreach (JCDContract contract in contracts)
            {
                OSMAdress currentContract = getOSMAdress(contract.name);

                GeoCoordinate currentstationGo = new GeoCoordinate(changeToDouble(currentContract.lat), changeToDouble(currentContract.lon));
                double distance = coordIndication.GetDistanceTo(currentstationGo);
                if (distance != 0 && (savedDistance == -1 || distance < savedDistance))
                {
                    savedDistance = distance;
                    currentClosestContract = contract.name;
                }
            }
            return currentClosestContract;

        }

        public double changeToDouble(string value)
        {
            value = value.Replace(".", ",");
            return Convert.ToDouble(value);
        }


        public List<JCDContract> cleanContractList(List<JCDContract> contracts)
        {
            contracts.RemoveAll(x => x.cities == null); // suppression dans la liste des contracts qui ne sont pas des villes 
            return contracts;
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
        public string commercial_name { get; set; }
        public List<string> cities { get; set; }
        public string country_code { get; set; }

        public override string ToString()
        {
            return
                $"{nameof(name)}: {name}, {nameof(commercial_name)}: {commercial_name}, {nameof(cities)}: {cities}, {nameof(country_code)}: {country_code}";
        }
    }

    public class JCDStation
    {
        public int number { get; set; }
        public string name { get; set; }
        public Position position { get; set; }
    }
    public class Address
    {
        public string town { get; set; }
        public string city { get; set; }
        public string village { get; set; }
    }

    public class OSMAdress
    {
        public string lat { get; set; }
        public string lon { get; set; }
        public string display_name { get; set; }
        public Address address { get; set; }
    }



}
