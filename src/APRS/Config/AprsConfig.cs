using System.Configuration;

namespace APRS
{
    public class AprsConfig : ConfigurationSection
    {
        [ConfigurationProperty("uri", DefaultValue = "noam.aprs2.net", IsRequired = false)]
        public string URI
        {
            get
            {
                return this["uri"] as string;
            }
        }

        [ConfigurationProperty("port", DefaultValue = 14580, IsRequired = false)]
        public int Port
        {
            get
            {
                return (int) this["port"];
            }
        }

        [ConfigurationProperty("callsign", DefaultValue = "0", IsRequired = false)]
        public string Callsign
        {
            get
            {
                return this["callsign"] as string;
            }
        }

        [ConfigurationProperty("password", DefaultValue = "-1", IsRequired = false)]
        public string Password
        {
            get
            {
                return this["password"] as string;
            }
        }

        [ConfigurationProperty("filter", DefaultValue = "s/'/`", IsRequired = false)]
        public string Filter
        {
            get
            {
                return this["filter"] as string;
            }
        }


        public static AprsConfig GetConfig()
        {
            return ConfigurationManager.GetSection("AprsConfig") as AprsConfig;
        }
    }
}