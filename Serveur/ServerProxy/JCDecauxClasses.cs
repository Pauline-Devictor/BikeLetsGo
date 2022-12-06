using System.Collections.Generic;

namespace ServerProxy
{

    public class Station
    {
        public string name { get; set; }
        public int number { get; set; }
        public Position position { get; set; }

        public string address { get; set; }
        public string contractName { get; set;}
        public TotalStands totalStands { get; set; }
        public override string ToString()
        {
            return
                $"{nameof(name)}: {name}, {nameof(totalStands)}: {totalStands.availabilities.stands}";
        }
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
    public class Availabilities
    {
        public int bikes { get; set; }
        public int stands { get; set; }
        public int mechanicalBikes { get; set; }
        public int electricalBikes { get; set; }
        public int electricalInternalBatteryBikes { get; set; }
        public int electricalRemovableBatteryBikes { get; set; }

        public override string ToString()
        {
            return
                $"{nameof(bikes)}: {bikes}, {nameof(stands)}: {stands}, {nameof(mechanicalBikes)}: {mechanicalBikes}, {nameof(electricalBikes)}: {electricalBikes}, {nameof(electricalInternalBatteryBikes)}: {electricalInternalBatteryBikes}, {nameof(electricalRemovableBatteryBikes)}: {electricalRemovableBatteryBikes}";
        }
    }
    public class TotalStands
    {
        public Availabilities availabilities { get; set; }
        public int capacity { get; set; }

        public override string ToString()
        {
            return $"{nameof(availabilities)}: {availabilities}, {nameof(capacity)}: {capacity}";
        }
    }
}
