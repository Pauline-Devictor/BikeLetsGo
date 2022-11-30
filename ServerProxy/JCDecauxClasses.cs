using System.Collections.Generic;

namespace ServerProxy
{

    public class Station
    {
        public string name { get; set; }
        public int number { get; set; }
        public Position position { get; set; }

        public string address { get; set; }
        public string contractName { get; set; }

    }

    public class Position
    {
        public double latitude { get; set; }
        public double longitude { get; set; }
    }

    public class JCDContract
    {
        public string name { get; set; }
        public string commercial_name { get; set; }
        public List<string> cities { get; set; }
        public string country_code { get; set; }

        public override string ToString()
        {
            return
                $"{nameof(name)}: {name}, {nameof(commercial_name)}: {commercial_name}, {nameof(cities)}: {cities}";
        }
    }
}
