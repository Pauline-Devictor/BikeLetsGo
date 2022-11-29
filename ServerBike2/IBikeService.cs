using System.Collections.Generic;
using System.ServiceModel;

namespace RoutingServerBike
{
    [ServiceContract]
    public interface IBikeService
    {
        [OperationContract]
        string getItinerary(string departure, string arrival);
    }

}
