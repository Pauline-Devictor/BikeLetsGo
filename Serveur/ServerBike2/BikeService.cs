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
        OpenRouteServiceHelper orsHelper = new OpenRouteServiceHelper(client);
        private JCDecauxServiceClient proxy = new JCDecauxServiceClient();

        private readonly ActiveMQService activeMQ = new ActiveMQService();


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

            string msg = "";
            if (!determineOptimizedItinerary(depart, arrivee, stations))
            {
                msg = "\n------------------------------\nAttention ! Le point de départ et d'arrivée sont très loin des stations de vélos ! "
                    + "\nIl vaudrait peut être mieux chercher un autre moyen de transport \n------------------------------";
            }

            if (stations == null)
            {
                return "Pas de vélos disponibles pour ce trajet";
            }
            return prepareMessage(stations, depart, arrivee, detailled) + msg;

        }
        //Determine Whether it's too long or not to go to the stations
        //Return true if it's not too far ; False if distance to the stations == distance(departure,arrival) *2
        private bool determineOptimizedItinerary(OSMAdress depart, OSMAdress arrivee, Station[] stations)
        {
            GeoCoordinate geoDeparture = CoordHelper.createGeocoordinate(depart);
            GeoCoordinate geoArrival = CoordHelper.createGeocoordinate(depart);
            Feature distanceDepartureArrival = orsHelper.getFootItineraryDetails(geoDeparture,geoArrival);
            Feature distanceDepatureToBike = orsHelper.getFootItineraryDetails(geoDeparture, new GeoCoordinate(stations[0].position.latitude, stations[0].position.longitude));
            Feature distanceArrivalToBike = orsHelper.getFootItineraryDetails(geoArrival, new GeoCoordinate(stations[1].position.latitude, stations[1].position.longitude));
            return !(CoordHelper.calculateDistance(distanceDepartureArrival) * 2 < CoordHelper.calculateDistance(distanceArrivalToBike) + CoordHelper.calculateDistance(distanceDepatureToBike));
        }
      

        private string getDetailledItinerary(Station[] stations, OSMAdress depart, OSMAdress arrivee)
        {
            GeoCoordinate departure = new GeoCoordinate(CoordHelper.changeToDouble(depart.lat), CoordHelper.changeToDouble(depart.lon));
            GeoCoordinate arrival = new GeoCoordinate(CoordHelper.changeToDouble(arrivee.lat), CoordHelper.changeToDouble(arrivee.lon));

            GeoCoordinate arret1 = new GeoCoordinate(stations[0].position.latitude, stations[0].position.longitude);
            GeoCoordinate arret2 = new GeoCoordinate(stations[1].position.latitude, stations[1].position.longitude);

            Feature feature0 = orsHelper.getFootItineraryDetails(departure, arret1);
            Feature feature1 = orsHelper.getBikeItineraryDetails(arret1, arret2);
            Feature feature2 = orsHelper.getFootItineraryDetails(arret2, arrival);

            List<Step> steps0 = feature0.properties.segments[0].steps;
            List<List<double>> coordinates0 = feature0.geometry.coordinates;
            List<Step> steps1 = feature1.properties.segments[0].steps;
            List<List<double>> coordinates1 = feature1.geometry.coordinates;
            List<Step> steps2 = feature2.properties.segments[0].steps;
            List<List<double>> coordinates2 = feature2.geometry.coordinates;


            string details = getMapDetails(stations, depart,arrivee);
            string instructions = "";
            instructions += "------------------------------\n" + "Départ de " + depart.display_name + "\n------------------------------";
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
            return details + "stop" + instructions;

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
                OSMAdress adress = StringHelper.changeFormat(adresses[0]);
                return adress;
            }
            else { throw new Exception(); }
        }


        private Station[] findClosestStations(OSMAdress departurePoint, OSMAdress arrivalPoint, string closestContract)
        {
            GeoCoordinate departure = CoordHelper.createGeocoordinate(departurePoint);
            GeoCoordinate arrival = CoordHelper.createGeocoordinate(arrivalPoint);

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
        public string findClosestContract(OSMAdress depart)
        {
            GeoCoordinate departGeo = CoordHelper.createGeocoordinate(depart);

            JCDContract[] contracts = proxy.getContracts();
            contracts = cleanContractList(contracts);

            double savedDistance = -1;
            String currentClosestContract = null;
            GeoCoordinate currentPosition = new GeoCoordinate(CoordHelper.changeToDouble(depart.lat), CoordHelper.changeToDouble(depart.lon));

            foreach (JCDContract contract in contracts)
            {
                if (StringHelper.stringCompare(depart, contract))
                {
                    OSMAdress currentContract = getOSMAdress(contract.name);

                    GeoCoordinate currentstationGo = new GeoCoordinate(CoordHelper.changeToDouble(currentContract.lat), CoordHelper.changeToDouble(currentContract.lon));
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

       


        private JCDContract[] cleanContractList(JCDContract[] contracts)
        {
            contracts = contracts.Where(x => x.cities != null).ToArray();
            // suppression dans la liste des contracts qui ne sont pas des villes 
            return contracts;
        }

       
     

        private string prepareMessage(Station[] stations, OSMAdress depart, OSMAdress arrivee, bool detailled)
        {
            string message = "Prendre le vélo à " + stations[0].address + " - " + stations[0].contractName + ". \nDéposer le vélo à " + stations[1].address + " - " + stations[1].contractName;

            string adresses = "Depart de : " + depart.display_name + "\n" + message + " \nArrivée à " + arrivee.display_name + " ";
            string detailledInstructions = "";
            //string mapInfos = "";
            string mapInfos = depart.lat 
                + "," + depart.lon + "/" + CoordHelper.convertDoubleToCorrectString(stations[0].position.latitude) 
                + "," + CoordHelper.convertDoubleToCorrectString(stations[0].position.longitude) + "/" + CoordHelper.convertDoubleToCorrectString(stations[1].position.latitude) 
                + "," + CoordHelper.convertDoubleToCorrectString(stations[1].position.longitude) + "/" + arrivee.lat 
                + "," + arrivee.lon + "test";
            if (detailled)
            {
                detailledInstructions = getDetailledItinerary(stations, depart, arrivee);
                if (detailledInstructions.Equals("Impossible de trouver un trajet détaillé pour ce parcours"))
                    return mapInfos + detailledInstructions + "\n" + adresses;
            }
            else
            {
                detailledInstructions = createNonDetailledMessage(stations,depart,arrivee);
            }
            return mapInfos + detailledInstructions;
        }

        private string createNonDetailledMessage(Station[] stations, OSMAdress depart, OSMAdress arrivee)
        {
            string details  = getMapDetails(stations, depart, arrivee);
            string message = "Adresse de départ : " + depart.display_name;
            message += "\nPrendre un vélo à " + stations[0].name;
            message += "\nDéposer le vélo à " + stations[1].name;
            message += "\nSe diriger vers la dernière destination : " + arrivee.display_name;
            return details +  "stop" + message;
            
        }

        private string getMapDetails(Station[] stations, OSMAdress depart, OSMAdress arrivee)
        {
            GeoCoordinate departure = new GeoCoordinate(CoordHelper.changeToDouble(depart.lat), CoordHelper.changeToDouble(depart.lon));
            GeoCoordinate arrival = new GeoCoordinate(CoordHelper.changeToDouble(arrivee.lat), CoordHelper.changeToDouble(arrivee.lon));

            GeoCoordinate arret1 = new GeoCoordinate(stations[0].position.latitude, stations[0].position.longitude);
            GeoCoordinate arret2 = new GeoCoordinate(stations[1].position.latitude, stations[1].position.longitude);

            Feature feature0 = orsHelper.getFootItineraryDetails(departure, arret1);
            Feature feature1 = orsHelper.getBikeItineraryDetails(arret1, arret2);
            Feature feature2 = orsHelper.getFootItineraryDetails(arret2, arrival);

            List<Step> steps0 = feature0.properties.segments[0].steps;
            List<List<double>> coordinates0 = feature0.geometry.coordinates;
            List<Step> steps1 = feature1.properties.segments[0].steps;
            List<List<double>> coordinates1 = feature1.geometry.coordinates;
            List<Step> steps2 = feature2.properties.segments[0].steps;
            List<List<double>> coordinates2 = feature2.geometry.coordinates;

            String details = "";
            foreach (List<double> lc in coordinates0)
            {
                foreach (double d in lc)
                {
                    details += CoordHelper.convertDoubleToCorrectString(d) + ",";
                }
                details = details.Substring(0, details.Length - 2) + "/";
            }
            foreach (List<double> lc in coordinates1)
            {
                foreach (double d in lc)
                {
                    details += CoordHelper.convertDoubleToCorrectString(d) + ",";
                }
                details = details.Substring(0, details.Length - 2) + "/";
            }
            foreach (List<double> lc in coordinates2)
            {
                foreach (double d in lc)
                {
                    details += CoordHelper.convertDoubleToCorrectString(d) + ",";
                }
                details = details.Substring(0, details.Length - 2) + "/";
            }
            details = details.Substring(0, details.Length - 2);

            //TODO mettre un cas pour si arret1 == arret2
            if (steps0 == null || steps1 == null || steps2 == null)
            { return "Impossible de trouver un trajet détaillé pour ce parcours"; }
            return details;
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
}

