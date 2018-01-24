using System;
using System.Windows;

namespace AprsTracker.ViewModel
{
    public class MapLocationViewModel : BaseViewModel
    {
        private double _Latitude;
        private double _Longitude;

        private double _Altitude;
        private double _TargetAltitude;

        public event EventHandler TrackingObject;

        public double Latitude
        {
            get
            {
                return _Latitude;
            }
            set
            {
                if (_Latitude == value)
                    return;
                _Latitude = value;
                OnPropertyChanged("Latitude");
                OnPropertyChanged("LatLong");
            }
        }

        public double Longitude
        {
            get
            {
                return _Longitude;
            }
            set
            {
                if (_Longitude == value)
                    return;
                _Longitude = value;
                OnPropertyChanged("Longitude");
                OnPropertyChanged("LatLong");
            }
        }

        public Point LatLong
        {
            get { return new Point(Latitude, Longitude); }
        }
        
        public double Altitude
        {
            get
            {
                return _Altitude;
            }
            set
            {
                if (_Altitude == value)
                    return;
                _Altitude = value;
                OnPropertyChanged("Altitude");
                OnPropertyChanged("ScaledAltitude");
            }
        }

        public double ScaledAltitude
        {
            get
            {
                return Math.Log(TargetAltitude, 2);
            }
            set
            {
                TargetAltitude = Math.Pow(2, value);
            }
        }

        public double TargetAltitude
        {
            get { return _TargetAltitude; }
            set
            {
                if (_TargetAltitude == value)
                    return;
                _TargetAltitude = value;
                OnPropertyChanged("Altitude");

                Altitude = value;
            }
        }

        public void TrackObject(TrackedObjectViewModel trackedObjectVM)
        {
            if (trackedObjectVM.LatestPacketInfoVM.PacketInfo.Latitude == null ||
                trackedObjectVM.LatestPacketInfoVM.PacketInfo.Longitude == null)
            {
                throw new ArgumentException("Lattitude and Longitude cannot be null.", "trackedObjectVM");
            }

            Latitude = trackedObjectVM.LatestPacketInfoVM.PacketInfo.Latitude.Value;
            Longitude = trackedObjectVM.LatestPacketInfoVM.PacketInfo.Longitude.Value;

            if (TrackingObject != null)
                TrackingObject(this, EventArgs.Empty);
        }

    }
}
