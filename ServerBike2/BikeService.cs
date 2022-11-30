using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using System.Device.Location;
using System.Text.Json;
using ServerBike2.ServiceReference1;
using System.Linq;
using System.ServiceModel.PeerResolvers;

namespace RoutingServerBike
{
    public class BikeService : IBikeService
    {

        static readonly HttpClient client = new HttpClient();
        private JCDecauxServiceClient proxy = new JCDecauxServiceClient();


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
                return "Addresses non trouvées, merci de réessayer";
            }
            //on peut optimiser en recherchant un contrat avec le même nom que la ville ou village du départ ou plus proche
            //On pourra affiner ensuite en gardant une liste des contrats proche du départ puis en regardant dans cette liste les contrats proche de l'arrivée
            //Puis en cherchant une ville du même contrat la plus proche de l'arrivée
  

            //Chercher pour tous les contrats s'ils contiennent des villes proches de l'arrivée & départ
            //sinon plus proche du départ
            string closestContractToDeparture = findClosestContract(depart,arrivee);
            if(closestContractToDeparture == null) { return "Pas de vélo possible entre ces deux destinations"; }
            //string closestContractToArrival = findClosestContract(arrivee);

            //Faire une seule boucle de recherche pour opti le temps?
            string arrets = findClosestStations(depart ,arrivee, askStationsOfAContract(closestContractToDeparture));
           // string arret2 = findClosestStation(arrivee, askStationsOfAContract(closestContractToDeparture));
            //string arret2 = findClosestStation(arrivee, askStationsOfAContract(closestContractToArrival));
            
            string adresses = "Depart de : " + depart.display_name + arrets + " \nArrivée à " + arrivee.display_name + " ";
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


        public Station[] askStationsOfAContract(string contract)
        {
            Station[] stations = proxy.StationsOfAContract(contract);
            return stations;
        }

        public string findClosestStations(OSMAdress departurePoint,OSMAdress arrivalPoint, Station[] stations)
        {
            GeoCoordinate clientPosition = new GeoCoordinate(changeToDouble(departurePoint.lat), changeToDouble(departurePoint.lon));
            GeoCoordinate arrivalPosition = new GeoCoordinate(changeToDouble(arrivalPoint.lat), changeToDouble(arrivalPoint.lon));


            double savedDistanceDeparture = -1;
            double savedDistanceArrival = -1;
            Station currentClosestStation = null;
            Station depositStation = null;

            foreach (Station station in stations)
            {
                GeoCoordinate currentstationGo = new GeoCoordinate(station.position.latitude, station.position.longitude);

                double departureDistance = clientPosition.GetDistanceTo(currentstationGo);
                double lastPointDistance = arrivalPosition.GetDistanceTo(currentstationGo);
                if (departureDistance != 0 && (savedDistanceDeparture == -1 || departureDistance < savedDistanceDeparture))
                {
                    savedDistanceDeparture = departureDistance;
                    currentClosestStation = station;
                }
                if(lastPointDistance!=0 && (savedDistanceArrival == -1) || lastPointDistance < savedDistanceArrival)
                {
                    savedDistanceArrival = lastPointDistance;
                    depositStation = station;
                }
            }
            string message = "\nPrendre le vélo à " + currentClosestStation.name + ". \nDéposer le vélo à " + depositStation.name;
            return message;
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
        public string findClosestContract(OSMAdress depart,OSMAdress arrivee)
        {
            
            GeoCoordinate departGeo = createGeocoordinate(depart);
            GeoCoordinate arriveeGeo = createGeocoordinate(arrivee);

            JCDContract[] contracts = proxy.getContracts();
            contracts = cleanContractList(contracts);
   
            double savedDistance = -1;
            String currentClosestContract = null;
            GeoCoordinate currentPosition = new GeoCoordinate(changeToDouble(depart.lat), changeToDouble(depart.lon));

            foreach (JCDContract contract in contracts)
            {
                Console.WriteLine(contract.name);
               if(depart.address.city == contract.name) { 
                OSMAdress currentContract = getOSMAdress(contract.name);


                GeoCoordinate currentstationGo = new GeoCoordinate(changeToDouble(currentContract.lat), changeToDouble(currentContract.lon));
                double distance = departGeo.GetDistanceTo(currentstationGo);
                if (distance != 0 && (savedDistance == -1 || distance < savedDistance))
                {
                    savedDistance = distance;
                    currentClosestContract = contract.name;
                }
                }
            }
            return currentClosestContract;

        }

        public double changeToDouble(string value)
        {
            value = value.Replace(".", ",");
            return Convert.ToDouble(value);
        }


        public JCDContract[] cleanContractList(JCDContract[] contracts)
        {
            contracts = contracts.Where(x => x.cities != null).ToArray();
             // suppression dans la liste des contracts qui ne sont pas des villes 
            return contracts;
        }

        public GeoCoordinate createGeocoordinate(OSMAdress position)
        {
            double positionLat = changeToDouble(position.lat);
            double positionLon = changeToDouble(position.lon);
            return new GeoCoordinate(positionLat, positionLon);
        }
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
