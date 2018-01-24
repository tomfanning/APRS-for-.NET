using System.Configuration;

namespace AprsTracker
{
    public class AprsTrackerConfig : ConfigurationSection
    {
        [ConfigurationProperty("startLocation", IsRequired = true)]
        public MapLocationConfig StartLocation
        {
            get
            {
                return this["startLocation"] as MapLocationConfig;
            }
        }

        public static AprsTrackerConfig GetConfig()
        {
            return ConfigurationManager.GetSection("AprsTrackerConfig") as AprsTrackerConfig;
        }
    }
}