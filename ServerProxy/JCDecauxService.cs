using System;
using System.Collections.Generic;
using System.Device.Location;

namespace ServerProxy
{
    public class JCDecauxService : IJCDecauxService
    {

        private const string ApiKey = "07a2f22eaa786639241a704c091d22c6875b5809";
        private static readonly string KeyUri = "apiKey="+ApiKey;
        private const string Uri = "https://api.jcdecaux.com/vls/v3/";

        private GenericProxyCache<List<JCDContract>> contractsCache = new GenericProxyCache<List<JCDContract>>();
        private GenericProxyCache<List<Station>> stationsCache = new GenericProxyCache<List<Station>>();

        public List<JCDContract> getContracts()
        {
            var reqString = Uri + "contracts" + "?" + KeyUri;
            return contractsCache.Get(reqString);
        }

        public List<Station> closestStationsOfAContract(string contractName, GeoCoordinate departure, GeoCoordinate arrival)
        {
            var reqString = Uri + "stations?contract=" + contractName + "&" + KeyUri;
            List<Station> closeststation = findClosestStation(stationsCache.Get(reqString, 6 * 60), departure, arrival);
            return closeststation; //2e arg permet de garder le cache pdt 6min
        }

        private List<Station> findClosestStation(List<Station> stations,GeoCoordinate departure, GeoCoordinate arrival)
        {
            GeoCoordinate clientPosition = new GeoCoordinate(departure.Latitude, departure.Longitude);
            GeoCoordinate arrivalPosition = new GeoCoordinate(arrival.Latitude, arrival.Longitude);

            double savedDistanceDeparture = -1;
            double savedDistanceArrival = -1;
            Station currentClosestStation = null;
            Station depositStation = null;

            foreach (Station station in stations)
            {
                GeoCoordinate currentstationGo = new GeoCoordinate(station.position.latitude, station.position.longitude);

                double departureDistance = clientPosition.GetDistanceTo(currentstationGo);
                double lastPointDistance = arrivalPosition.GetDistanceTo(currentstationGo);
                if ((departureDistance != 0 && (savedDistanceDeparture == -1 || departureDistance < savedDistanceDeparture))&&isABikeAvailable(station))
                {
                    savedDistanceDeparture = departureDistance;
                    currentClosestStation = station;
                }
                if ((lastPointDistance != 0 && (savedDistanceArrival == -1) || lastPointDistance < savedDistanceArrival)&&isAParkingSlot(station))
                {
                    savedDistanceArrival = lastPointDistance;
                    depositStation = station;
                }
            }
            List <Station> closestStations = new List<Station>();
            closestStations.Add(currentClosestStation);
            closestStations.Add(depositStation);
            return closestStations;
        }


        private bool isABikeAvailable(Station station)
        {
            return (station.totalStands.availabilities.bikes != 0);
        }
        private bool isAParkingSlot(Station station)
        {
            return (station.totalStands.availabilities.stands != 0);
        }
    }
}
