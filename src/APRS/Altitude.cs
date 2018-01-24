using System;

namespace APRS
{
    public class Altitude
    {
        private const int BASELINE_DEPTH_BELOW_SEALEVEL = 10000;
        private const double METER_TO_FOOT_CONVERSION_FACTOR = 3.2808399;

        private AltitudeUnit _AltitudeUnit;
        private int _Magnitude;

        private Altitude()
        {
            
        }

        public static Altitude FromMetersAboveBaseline(int metersAboveBaseline)
        {
            return new Altitude
            {
                _AltitudeUnit = AltitudeUnit.Meters, 
                _Magnitude = metersAboveBaseline - BASELINE_DEPTH_BELOW_SEALEVEL
            };
        }

        public static Altitude FromMetersAboveSeaLevel(int metersAboveSeaLevel)
        {
            return new Altitude
            {
                _AltitudeUnit = AltitudeUnit.Meters,
                _Magnitude = metersAboveSeaLevel
            };
        }
        public static Altitude FromFeetAboveSeaLevel(int feetAboveSeaLevel)
        {
            return new Altitude
            {
                _AltitudeUnit = AltitudeUnit.Feet,
                _Magnitude = feetAboveSeaLevel
            };
        }

        public int MetersAboveBaseline
        {
            get
            {
                return MetersAboveSeaLevel + BASELINE_DEPTH_BELOW_SEALEVEL;
            }
        }

        public int MetersAboveSeaLevel
        {
            get
            {
                return _AltitudeUnit == AltitudeUnit.Meters
                    ? _Magnitude
                    : Convert.ToInt32(_Magnitude / METER_TO_FOOT_CONVERSION_FACTOR);
            }
        }

        public int FeetAboveSeaLevel
        {
            get
            {
                return _AltitudeUnit == AltitudeUnit.Feet
                    ? _Magnitude
                    : Convert.ToInt32(_Magnitude * METER_TO_FOOT_CONVERSION_FACTOR);
            }
        }

        public override string ToString()
        {
            return string.Format("{0:d}ft MSL", FeetAboveSeaLevel);
        }
    }
}