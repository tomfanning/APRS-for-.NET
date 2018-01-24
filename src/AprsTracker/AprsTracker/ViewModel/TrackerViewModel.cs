using System;
using System.Linq;
using System.Collections.ObjectModel;
using System.Windows;
using APRS;

namespace AprsTracker.ViewModel
{
    public class TrackerViewModel : BaseViewModel
    {

        private readonly PacketListener _PacketListener = new PacketListener();

        private readonly ObservableCollection<TrackedObjectViewModel> _TrackedObjectList = new ObservableCollection<TrackedObjectViewModel>();
        private readonly MapLocationViewModel _MapLocationVM = new MapLocationViewModel
        {
            Latitude = AprsTrackerConfig.GetConfig().StartLocation.Latitude,
            Longitude = AprsTrackerConfig.GetConfig().StartLocation.Longitude,
            TargetAltitude = AprsTrackerConfig.GetConfig().StartLocation.Altitude
        };

        private TrackedObjectViewModel _CurrentlyTrackedObject;

        public event EventHandler<PacketInfoEventArgs> CurrentllyTrackedObjectPacketReceived;

        public TrackerViewModel()
        {
            _PacketListener.PacketReceived += _PacketListener_PacketReceived;
            _PacketListener.Start();
        }

        private void _PacketListener_PacketReceived(object sender, PacketInfoEventArgs e)
        {
            Application.Current.Dispatcher.Invoke((Action) (() =>
            {
                var packetInfo = e.PacketInfo;

                if (e.PacketInfo.Latitude == null ||
                    e.PacketInfo.Longitude == null)
                {
                    return;
                }

                TrackedObjectViewModel trackedObject;
                lock (_TrackedObjectList)
                {
                    trackedObject = (from obj in _TrackedObjectList where obj.LatestPacketInfoVM.PacketInfo.Callsign == packetInfo.Callsign select obj).SingleOrDefault();

                    if (trackedObject == null)
                    {
                        trackedObject = new TrackedObjectViewModel(packetInfo);
                        _TrackedObjectList.Add(trackedObject);
                    }
                    else
                        trackedObject.Update(packetInfo);
                }

                if (CurrentlyTrackedObject == null)
                    CurrentlyTrackedObject = trackedObject;

                if (trackedObject == CurrentlyTrackedObject)
                {
                    MapLocationVM.TrackObject(trackedObject);
                    if (CurrentllyTrackedObjectPacketReceived != null)
                        CurrentllyTrackedObjectPacketReceived(this, new PacketInfoEventArgs(packetInfo));
                }
            }));
        }

        public ObservableCollection<TrackedObjectViewModel> TrackedObjectList
        {
            get { return _TrackedObjectList; }
            //set
            //{
            //    if (_TrackedObjectList == value)
            //        return;

            //    _TrackedObjectList = value;
            //    OnPropertyChanged("TrackedObjectList");
            //}
        }

        public MapLocationViewModel MapLocationVM
        {
            get { return _MapLocationVM; }
        }

        public TrackedObjectViewModel CurrentlyTrackedObject
        {
            get { return _CurrentlyTrackedObject; }
            set
            {
                if (_CurrentlyTrackedObject == value)
                    return;
                _CurrentlyTrackedObject = value;
                OnPropertyChanged("CurrentlyTrackedObject");
                MapLocationVM.TrackObject(value);
            }
        }
    }
}
