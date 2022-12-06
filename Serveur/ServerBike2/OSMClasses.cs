using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServerBike2
{
    public class Address
    {
        public string town { get; set; }
        public string city { get; set; }
        public string village { get; set; }
        public string municipality { get; set; }
        public override string ToString()
        {
            return
            $"{nameof(town)}: {town}, {nameof(city)}: {city}, {nameof(village)}: {village}, {nameof(municipality)}: {municipality}";
        }
    }

    public class OSMAdress
    {
        public string lat { get; set; }
        public string lon { get; set; }
        public string display_name { get; set; }
        public Address address { get; set; }
    }

}
