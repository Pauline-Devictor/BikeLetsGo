using System.Collections.Generic;
using System.ServiceModel;

namespace RoutingServerBike
{
    [ServiceContract()]
    public interface IBikeService
    {
        [OperationContract()]
        //Récupère tous les contrats
        string getContracts();

        [OperationContract()]
        string getStations(string contract);
        [OperationContract()]
        string getStation(string station);
        [OperationContract()]
        string getStationInGivenCityCloseToUs(double latitude, double longitude, string city);
    }

}
