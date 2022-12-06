using System.Collections.Generic;
using System.ServiceModel;
using System.Device.Location;

namespace ServerProxy
{
    [ServiceContract]
    public interface IJCDecauxService
    {
        [OperationContract]
        List<JCDContract> getContracts();
        [OperationContract]
        List<Station> closestStationsOfAContract(string contractName, GeoCoordinate departure, GeoCoordinate arrival);
    }
    
}
