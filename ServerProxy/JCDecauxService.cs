using System.Collections.Generic;

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

        public List<Station> StationsOfAContract(string contractName)
        {
            var reqString = Uri + "stations?contract=" + contractName + "&" + KeyUri;
            return stationsCache.Get(reqString, 6 * 60); //2e arg permet de garder le cache pdt 6min
        }
    }
}
