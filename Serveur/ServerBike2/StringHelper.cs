using ServerBike2.ServiceReference1;
using System;
using System.Linq;


namespace ServerBike2
{
    public static class StringHelper
    {
        public static bool stringCompare(OSMAdress osmAdress, JCDContract contract)
        {
            if (osmAdress.address.city != null)
            {
                if (osmAdress.address.city.ToLower().Trim().Equals(contract.name) || contract.cities.Contains(osmAdress.address.city))
                {
                    return true;
                }
            }
            if (osmAdress.address.town != null)
            {
                if (osmAdress.address.town.ToLower().Trim().Equals(contract.name) || contract.cities.Contains(osmAdress.address.town))
                {
                    return true;
                }
            }
            if (osmAdress.address.village != null)
            {
                if (osmAdress.address.village.ToLower().Trim().Equals(contract.name) || contract.cities.Contains(osmAdress.address.village))
                {
                    return true;
                }
            }
            if (osmAdress.address.municipality != null)
            {
                if (osmAdress.address.municipality.ToLower().Trim().Equals(contract.name) || contract.cities.Contains(osmAdress.address.municipality))
                {
                    return true;
                }
            }
            return false;
        }

        private static string replaceCharacter(string input)
        {
            input = input.Replace("ç", "c");
            input = input.Replace("é", "e");
            input = input.Replace("è", "e");
            return input;
        }

        public static OSMAdress changeFormat(OSMAdress adress)
        {
            if (adress.address.town != null)
            {
                adress.address.town = replaceCharacter(adress.address.town);
            }
            if (adress.address.city != null)
            {
                adress.address.city = replaceCharacter(adress.address.city);
            }
            if (adress.address.village != null)
            {
                adress.address.village = replaceCharacter(adress.address.village);
            }
            if (adress.address.municipality != null)
            {
                adress.address.municipality = replaceCharacter(adress.address.municipality);
            }
            return adress;
        }


    }
}
