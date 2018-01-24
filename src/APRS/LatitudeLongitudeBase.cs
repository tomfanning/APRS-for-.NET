﻿using System;

namespace APRS
{
    public abstract class LatitudeLongitudeBase
    {
        protected LatitudeLongitudeBase(PositionSign sign, short degrees, short minutes, double seconds)
            : this(sign, degrees, minutes, seconds, 0)
        {
        }

        protected LatitudeLongitudeBase(PositionSign sign, short degrees, short minutes, double seconds, short ambiguity)
        {
            Sign = sign;
            Vector = new Heading(degrees, minutes, seconds);
            Ambiguity = ambiguity;
        }

        public PositionSign Sign { get; set; }
        public Heading Vector { get; private set; }

        public double Value
        {
            get
            {
                return (Sign == PositionSign.Positive ? 1 : -1) * Vector.ToDegrees();
            }
            set
            {
                Sign = value >= 0 ? PositionSign.Positive : PositionSign.Negative;
                Vector.SetDegrees(Math.Abs(value));
            }
        }

        public short Ambiguity { get; set; }

        public double AbsoluteValue
        {
            get { return Math.Abs(Value); }
        }

        public override string ToString()
        {
            return string.Format("{0}{1}", 
                Sign == PositionSign.Positive ? "" : "-",
                Vector);
        }
    }
}