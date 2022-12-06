using ServerBike2.ServiceReference1;
using System;
using System.Collections.Generic;
using System.Device.Location;
using System.Linq;

namespace ServerBike2
{
    public static class CoordHelper
    {
        public static double calculateDistance(Feature featureToEvaluate)
        {
            double distance = 0;
            List<Segment> segments = featureToEvaluate.properties.segments;
            foreach (Segment segment in segments)
            {
                distance += segment.distance;
            }
            return distance;
        }


        public static double changeToDouble(string value)
        {
            value = value.Replace(".", ",");
            return Convert.ToDouble(value);
        }


        public static GeoCoordinate createGeocoordinate(OSMAdress position)
        {
            double positionLat = changeToDouble(position.lat);
            double positionLon = changeToDouble(position.lon);
            return new GeoCoordinate(positionLat, positionLon);
        }


        public static string convertCoordToCorrectString(GeoCoordinate coord)
        {
            string a = Convert.ToString(coord.Longitude);
            a = a.Replace(",", ".");
            a += ",";
            string b = Convert.ToString(coord.Latitude);
            b = b.Replace(",", ".");
            a += b;
            return a;
        }

        public static string convertDoubleToCorrectString(double d)
        {
            string a = Convert.ToString(d);
            a = a.Replace(",", ".");
            return a;
        }

    }
}
