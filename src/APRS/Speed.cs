using System;

namespace APRS
{
    public class Speed
    {
        private Speed()
        {
            
        }

        public static Speed FromKnots(double knots)
        {
            return new Speed {Knots = knots};
        }

        public double Knots { get; private set; }

        public double MilesPerHour
        {
            get { return Knots * 1.15077945; }
        }

        public double KilometersPerHour
        {
            get { return Knots * 1.85200; }
        }

        public override string ToString()
        {
            return string.Format("{0}KTS", Math.Floor(Knots));
        }
    }
}