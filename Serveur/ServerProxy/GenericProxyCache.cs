using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Runtime.Caching;
using System.Linq; //pour le dictionnaire
using System.Text.Json;

namespace ServerProxy
{
    public class GenericProxyCache<T> where T : class
    {
        //A l'instanciation de la classe
        DateTimeOffset dt_default = ObjectCache.InfiniteAbsoluteExpiration;
        private static readonly HttpClient client = new HttpClient();
        //Permet de conserver les informations ainsi que la date de la requête
        //le string contient l'url de la requete
        private Dictionary<string, Tuple<T, DateTimeOffset>> cache = new Dictionary<string, Tuple<T, DateTimeOffset>>();

        //All get methods should do :
        //-If CacheItemName doesn't exist or has a null content
        //->then create a new T object and put it in the cache with CacheItemName as the corresponding key.


        //Si n'existe pas alors, prends la valeur actuelle de dt_default; par defaut = InfiniteAbsoluteExpiration
        public T Get(string cacheItemName)
        {
            return Get(cacheItemName, dt_default);
        }

        //Ici, Expiration Time = now + dt_seconds seconds
        public T Get(string cacheItemName, double dt_seconds)
        {
            return Get(cacheItemName, DateTimeOffset.Now.AddSeconds(dt_seconds));
        }

        //Expiration Time = dt
        public T Get(string cacheItemName, DateTimeOffset dt)
        {
            //On commence par mettre à jour le cache
            CleanCache();
           
            if (cache.ContainsKey(cacheItemName) && cache[cacheItemName].Item1 != null)
            {
                //Console.WriteLine($"Utilisation du cache {cacheItemName}");
                return cache[cacheItemName].Item1;
            }
            //Console.WriteLine($"requete vers serveur {cacheItemName}");
            createNewTObject(cacheItemName, dt);
            return cache[cacheItemName].Item1;
        }
        //Clear the cache of expired data
        private void CleanCache()
        {   
            cache = this.cache
                .Where(objet => objet.Value.Item2 > DateTime.Now)//si la date d'expiration > maintenant alors on garde
                .ToDictionary(objet => objet.Key, objet => objet.Value);//on remet le tout sous forme de dictionnaire
        }

        private void createNewTObject(string cacheItemName, DateTimeOffset dt)
        {
            var responseBody = client.GetStringAsync(cacheItemName);
            var objectToAdd = JsonSerializer.Deserialize<T>(responseBody.Result);
            //and put it in the cache with CacheItemName as the corresponding key"
            cache[cacheItemName] = Tuple.Create(objectToAdd, dt);
        }
    }
}
