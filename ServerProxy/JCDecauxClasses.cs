using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServerProxy
{

    public class Station
    {
        public string name { get; set; }
        public int number { get; set; }
        public Position position { get; set; }

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

    public class JCDStation
    {
        public int number { get; set; }
        public string name { get; set; }
        public Position position { get; set; }
    }
    public class Address
    {
        public string town { get; set; }
        public string city { get; set; }
        public string village { get; set; }
    }
}
