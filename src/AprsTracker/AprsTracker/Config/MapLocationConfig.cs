using System;
using System.Configuration;

namespace AprsTracker
{
    public class MapLocationConfig : ConfigurationElement
    {
        [ConfigurationProperty("latitude", IsRequired = true)]
        public double Latitude
        {
            get
            {
                return Convert.ToDouble(this["latitude"]);
            }
        }

        [ConfigurationProperty("longitude", IsRequired = true)]
        public double Longitude
        {
            get
            {
                return Convert.ToDouble(this["longitude"]);
            }
        }

        [ConfigurationProperty("altitude", IsRequired = true)]
        public double Altitude
        {
            get
            {
                return Convert.ToDouble(this["altitude"]);
            }
        }
    }
}