using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using System.Device.Location;
using System.Text.Json;
using ServerBike2.ServiceReference1;
using System.Linq;
using System.ServiceModel.PeerResolvers;
using System.Diagnostics.Contracts;
using System.Security.AccessControl;

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
            string closestContract = findClosestContract(depart, arrivee);
            if (closestContract == null) { return "Pas de vélo possible entre ces deux destinations"; }


            string arrets = findClosestStations(depart, arrivee, closestContract);



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


        public string findClosestStations(OSMAdress departurePoint, OSMAdress arrivalPoint,string closestContract )
        {
            GeoCoordinate departure = createGeocoordinate(departurePoint);
            GeoCoordinate arrival = createGeocoordinate(arrivalPoint);

            Station[] stations = proxy.closestStationsOfAContract(closestContract, departure, arrival);



            string message = "\nPrendre le vélo à " + stations[0].address + " - " + stations[0].contractName + ". \nDéposer le vélo à " + stations[1].address + " - " + stations[1].contractName;
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
        public string findClosestContract(OSMAdress depart, OSMAdress arrivee)
        {

            GeoCoordinate departGeo = createGeocoordinate(depart);            

            JCDContract[] contracts = proxy.getContracts();
            contracts = cleanContractList(contracts);

            double savedDistance = -1;
            String currentClosestContract = null;
            GeoCoordinate currentPosition = new GeoCoordinate(changeToDouble(depart.lat), changeToDouble(depart.lon));

            foreach (JCDContract contract in contracts)
            {
                if (stringCompare(depart, contract))
                {Console.WriteLine(contract.name);
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

        public bool stringCompare(OSMAdress osmAdress, JCDContract contract)
        {
            if (osmAdress.address.city != null)
            {
                if (osmAdress.address.city.ToLower().Equals(contract.name) || contract.cities.Contains(osmAdress.address.city))
                {
                    return true;
                }
                
            }
            if (osmAdress.address.town != null)
            {
                if (osmAdress.address.town.ToLower().Equals(contract.name) || contract.cities.Contains(osmAdress.address.town))
                {
                    return true;
                }
            }
            if (osmAdress.address.village != null)
            {
                if (osmAdress.address.village.ToLower().Equals(contract.name) || contract.cities.Contains(osmAdress.address.village))
                {
                    return true;
                }
            }
            if (osmAdress.address.municipality != null)
            {
                if (osmAdress.address.municipality.ToLower().Equals(contract.name) || contract.cities.Contains(osmAdress.address.municipality))
                {
                    return true;
                }
            }
            return false;
        }





}


        public class Address
        {
            public string town { get; set; }
            public string city { get; set; }
            public string village { get; set; }
            public string municipality { get; set; }
        }

        public class OSMAdress
        {
            public string lat { get; set; }
            public string lon { get; set; }
            public string display_name { get; set; }
            public Address address { get; set; }
        }



    }

