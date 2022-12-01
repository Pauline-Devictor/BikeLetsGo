using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using System.Device.Location;
using System.Text.Json;
using ServerBike2.ServiceReference1;
using System.Linq;
using ServerBike2;

namespace RoutingServerBike
{
    public class BikeService : IBikeService
    {

        static readonly HttpClient client = new HttpClient();
        private JCDecauxServiceClient proxy = new JCDecauxServiceClient();


        public string getItinerary(string departure, string arrival, bool detailled)
        {
            List<OSMAdress> adresses = new List<OSMAdress>();
            try
            {
                adresses = getOSMAdresses(departure, arrival);
            }
            catch (Exception e)
            {
                return "Addresses non trouvées, merci de réessayer";
            }
            OSMAdress depart = adresses[0];
            OSMAdress arrivee = adresses[1];

            string closestContract = findClosestContract(depart);
            if (closestContract == null) { return "Pas de vélo possible entre ces deux destinations"; }

            Station[] stations = findClosestStations(depart, arrivee, closestContract);
            if (stations == null)
            {
                return "Pas de vélos disponibles pour ce trajet";
            }
            return prepareMessage(stations, depart, arrivee, detailled);

        }

        private string getDetailledItinerary(Station[] stations, OSMAdress depart, OSMAdress arrivee, string itineraryDescription)
        {
            string instructions = "";
            GeoCoordinate departure = new GeoCoordinate(changeToDouble(depart.lat), changeToDouble(depart.lon));
            GeoCoordinate arrival = new GeoCoordinate(changeToDouble(arrivee.lat), changeToDouble(arrivee.lon));

            GeoCoordinate arret1 = new GeoCoordinate(stations[0].position.latitude, stations[0].position.longitude);
            GeoCoordinate arret2 = new GeoCoordinate(stations[1].position.latitude, stations[1].position.longitude);

            var steps0 = getFootItineraryDetails(departure, arret1);
            var steps1 = getBikeItineraryDetails(arret1, arret2);
            var steps2 = getFootItineraryDetails(arret2, arrival);
            //TODO mettre un cas pour si arret1 == arret2
            if (steps0 == null || steps1 == null || steps2 == null)
            { return "Impossible de trouver un trajet détaillé pour ce parcours"; }

            instructions += "\n" + "Départ de " + depart.display_name + "\n------------------------------";
            foreach (Step step in steps0)
            {
                instructions += "\n" + step.instruction;
            }
            instructions += "\n------------------------------\nArrivée à " + stations[0].name + ", prendre un vélo \n------------------------------";
            foreach (Step step in steps1)
            {
                instructions += "\n" + step.instruction;
            }
            instructions += "\n------------------------------\n Arrivée à " + stations[1].name + ", poser le vélo \n------------------------------";

            foreach (Step step in steps2)
            {
                instructions += "\n" + step.instruction;
            }
            instructions += "\n------------------------------\n Arrivée à destination à " + arrivee.display_name + "\n------------------------------";
            return instructions;

        }


        private OSMAdress getOSMAdress(string address)
        {
            client.DefaultRequestHeaders.Add("User-Agent", "RoutingServer");
            string query = "q=\"" + address.Replace(' ', '+') + "&format=jsonv2&limit=1&addressdetails=1";
            string url = "https://nominatim.openstreetmap.org/search.php";
            string response = callAPI(url, query).Result;
            List<OSMAdress> adresses = JsonSerializer.Deserialize<List<OSMAdress>>(response);
            if (adresses != null)
            {
                OSMAdress adress = changeFormat(adresses[0]);
                return adress;
            }
            else { throw new Exception(); }
        }


        private Station[] findClosestStations(OSMAdress departurePoint, OSMAdress arrivalPoint, string closestContract)
        {
            GeoCoordinate departure = createGeocoordinate(departurePoint);
            GeoCoordinate arrival = createGeocoordinate(arrivalPoint);

            Station[] stations = proxy.closestStationsOfAContract(closestContract, departure, arrival);
            if (stations[0] == null || stations[1] == null) { return null; }

            return stations;

        }

        static async Task<string> callAPI(string url, string query)
        {
            //HttpResponseMessage response = await client.GetAsync(url + "?" + query);
            //Uri uri = new Uri("https://nominatim.openstreetmap.org/search.php?q=111+avenue+des+pugets&format=json&limit=1&addressdetails=1");
            Uri uri = new Uri(url + "?" + query);

            HttpResponseMessage response = await client.GetAsync(uri);
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadAsStringAsync();
        }



        //Retourne la ville la plus proche des coordonnées données
        //TODO Diviser la methode
        public string findClosestContract(OSMAdress depart)
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
                {
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

        private double changeToDouble(string value)
        {
            value = value.Replace(".", ",");
            return Convert.ToDouble(value);
        }


        private JCDContract[] cleanContractList(JCDContract[] contracts)
        {
            contracts = contracts.Where(x => x.cities != null).ToArray();
            // suppression dans la liste des contracts qui ne sont pas des villes 
            return contracts;
        }

        private GeoCoordinate createGeocoordinate(OSMAdress position)
        {
            double positionLat = changeToDouble(position.lat);
            double positionLon = changeToDouble(position.lon);
            return new GeoCoordinate(positionLat, positionLon);
        }

        private bool stringCompare(OSMAdress osmAdress, JCDContract contract)
        {
            if (osmAdress.address.city != null)
            {
                if (osmAdress.address.city.ToLower().Trim().Equals(contract.name) || contract.cities.Contains(osmAdress.address.city))
                {
                    return true;
                }
            }
            if (osmAdress.address.town != null)
            {
                if (osmAdress.address.town.ToLower().Trim().Equals(contract.name) || contract.cities.Contains(osmAdress.address.town))
                {
                    return true;
                }
            }
            if (osmAdress.address.village != null)
            {
                if (osmAdress.address.village.ToLower().Trim().Equals(contract.name) || contract.cities.Contains(osmAdress.address.village))
                {
                    return true;
                }
            }
            if (osmAdress.address.municipality != null)
            {
                if (osmAdress.address.municipality.ToLower().Trim().Equals(contract.name) || contract.cities.Contains(osmAdress.address.municipality))
                {
                    return true;
                }
            }
            return false;
        }
        private OSMAdress changeFormat(OSMAdress adress)
        {
            if (adress.address.town != null)
            {
                adress.address.town = replaceCharacter(adress.address.town);
            }
            if (adress.address.city != null)
            {
                adress.address.city = replaceCharacter(adress.address.city);
            }
            if (adress.address.village != null)
            {
                adress.address.village = replaceCharacter(adress.address.village);
            }
            if (adress.address.municipality != null)
            {
                adress.address.municipality = replaceCharacter(adress.address.municipality);
            }
            return adress;
        }
        private string replaceCharacter(string input)
        {
            input = input.Replace("ç", "c");
            input = input.Replace("é", "e");
            input = input.Replace("è", "e");
            return input;
        }
        private List<Step> getBikeItineraryDetails(GeoCoordinate start, GeoCoordinate end)
        {
            // key ORS = 5b3ce3597851110001cf62482cb55505d51f40be9833ab07a19a9573
            string query, apiKey, url, response;
            client.DefaultRequestHeaders.Add("User-Agent", "RoutingServer");
            apiKey = "5b3ce3597851110001cf62482cb55505d51f40be9833ab07a19a9573";
            query = "api_key=" + apiKey + "&start=" + convertCoordToCorrectString(start) + "&end=" + convertCoordToCorrectString(end);
            //velo
            url = "https://api.openrouteservice.org/v2/directions/cycling-regular";
            try { response = callAPI(url, query).Result; }
            catch (Exception e) { return null; }
            List<Step> steps = JsonSerializer.Deserialize<Root>(response).features[0].properties.segments[0].steps;
            return steps;
        }
        public List<Step> getFootItineraryDetails(GeoCoordinate start, GeoCoordinate end)
        {
            // key ORS = 5b3ce3597851110001cf62482cb55505d51f40be9833ab07a19a9573

            string query, apiKey, url, response;
            client.DefaultRequestHeaders.Add("User-Agent", "RoutingServer");
            apiKey = "5b3ce3597851110001cf62482cb55505d51f40be9833ab07a19a9573";
            query = "api_key=" + apiKey + "&start=" + convertCoordToCorrectString(start) + "&end=" + convertCoordToCorrectString(end);
            url = "https://api.openrouteservice.org/v2/directions/foot-walking";
            try { response = callAPI(url, query).Result; }
            catch (Exception e) { return null; }
            var steps = JsonSerializer.Deserialize<Root>(response).features[0].properties.segments[0].steps;
            return steps;
        }

        private string convertCoordToCorrectString(GeoCoordinate coord)
        {
            string a = Convert.ToString(coord.Longitude);
            a = a.Replace(",", ".");
            a += ",";
            string b = Convert.ToString(coord.Latitude);
            b = b.Replace(",", ".");
            a += b;
            return a;
        }

        private string prepareMessage(Station[] stations, OSMAdress depart, OSMAdress arrivee, bool detailled)
        {
            string message = "Prendre le vélo à " + stations[0].address + " - " + stations[0].contractName + ". \nDéposer le vélo à " + stations[1].address + " - " + stations[1].contractName;

            string adresses = "Depart de : " + depart.display_name + "\n" + message + " \nArrivée à " + arrivee.display_name + " ";
            if (detailled)
            {
                string detailledInstructions = getDetailledItinerary(stations, depart, arrivee, adresses);
                if (detailledInstructions.Equals("Impossible de trouver un trajet détaillé pour ce parcours"))
                    return detailledInstructions + "\n" + adresses;
                return detailledInstructions;
            }
            return adresses;
        }

        private List<OSMAdress> getOSMAdresses(string departure, string arrival)
        {
            OSMAdress depart = getOSMAdress(departure);
            OSMAdress arrive = getOSMAdress(arrival);
            List<OSMAdress> list = new List<OSMAdress>();
            list.Add(depart);
            list.Add(arrive);
            return list;
        }
    }


    public class Address
    {
        public string town { get; set; }
        public string city { get; set; }
        public string village { get; set; }
        public string municipality { get; set; }
        public override string ToString()
        {
            return
            $"{nameof(town)}: {town}, {nameof(city)}: {city}, {nameof(village)}: {village}, {nameof(municipality)}: {municipality}";
        }
    }

    public class OSMAdress
    {
        public string lat { get; set; }
        public string lon { get; set; }
        public string display_name { get; set; }
        public Address address { get; set; }
    }


}

