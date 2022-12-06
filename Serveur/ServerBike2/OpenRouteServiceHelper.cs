using System;
using System.Device.Location;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;

namespace ServerBike2
{
    public class OpenRouteServiceHelper
    {
        static HttpClient client;
        static string apiKeyORS = "5b3ce3597851110001cf62482cb55505d51f40be9833ab07a19a9573";
        public OpenRouteServiceHelper(HttpClient serverClient)
        {
            client = serverClient;
        }
        public Feature getBikeItineraryDetails(GeoCoordinate start, GeoCoordinate end)
        {
            // key ORS = 5b3ce3597851110001cf62482cb55505d51f40be9833ab07a19a9573
            string query, url, response;
            client.DefaultRequestHeaders.Add("User-Agent", "RoutingServer");
            query = "api_key=" + apiKeyORS + "&start=" + CoordHelper.convertCoordToCorrectString(start) + "&end=" + CoordHelper.convertCoordToCorrectString(end);
            //velo
            url = "https://api.openrouteservice.org/v2/directions/cycling-regular";
            try { response = callAPI(url, query).Result; }
            catch (Exception e) { return null; }
            Feature feature = JsonSerializer.Deserialize<Root>(response).features[0];
            //Console.WriteLine(details);
            return feature;
        }
        public Feature getFootItineraryDetails(GeoCoordinate start, GeoCoordinate end)
        {
            // key ORS = 5b3ce3597851110001cf62482cb55505d51f40be9833ab07a19a9573

            string query, url, response;
            client.DefaultRequestHeaders.Add("User-Agent", "RoutingServer");
            query = "api_key=" + apiKeyORS + "&start=" + CoordHelper.convertCoordToCorrectString(start) + "&end=" + CoordHelper.convertCoordToCorrectString(end);
            url = "https://api.openrouteservice.org/v2/directions/foot-walking";
            try { response = callAPI(url, query).Result; }
            catch (Exception e) { return null; }
            Feature feature = JsonSerializer.Deserialize<Root>(response).features[0];

            return feature;
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
    }
}
