using System.Collections.Generic;
using System.ServiceModel;

namespace ServerProxy
{
    [ServiceContract]
    public interface IJCDecauxService
    {
        [OperationContract]
        List<JCDContract> getContracts();
        [OperationContract]
        List<Station> StationsOfAContract(string contractName);
    }
    
}
