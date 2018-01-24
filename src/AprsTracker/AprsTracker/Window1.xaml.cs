using System;
using System.Threading;
using System.Windows;
using APRS;
using AprsTracker.ViewModel;
using InfoStrat.VE;

namespace AprsTracker
{
    /// <summary>
    /// Interaction logic for Window1.xaml
    /// </summary>
    public partial class Window1
    {
        private TrackerViewModel _TrackerViewModel;

        public Window1()
        {
            InitializeComponent();
        }

        private void map_Loaded(object sender, RoutedEventArgs e)
        {
            _TrackerViewModel = new TrackerViewModel();
            _TrackerViewModel.PropertyChanged += _TrackerViewModel_PropertyChanged;
            _TrackerViewModel.MapLocationVM.TrackingObject += MapLocationVM_TrackingObject;
            _TrackerViewModel.CurrentllyTrackedObjectPacketReceived += _TrackerViewModel_CurrentllyTrackedObjectPacketReceived;
            DataContext = _TrackerViewModel;
        }

        void _TrackerViewModel_CurrentllyTrackedObjectPacketReceived(object sender, PacketInfoEventArgs e)
        {
            addPushPin(e.PacketInfo);
        }

        void _TrackerViewModel_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "CurrentlyTrackedObject")
            {
                map.Items.Clear();
                foreach (var packetInfoVM in _TrackerViewModel.CurrentlyTrackedObject.PacketInfoHistory)
                {
                    addPushPin(packetInfoVM.PacketInfo);
                }
            }
        }

        void addPushPin(PacketInfo packetInfo)
        {
            var latLong = new VELatLong(
               packetInfo.Latitude.Value,
               packetInfo.Longitude.Value);

            var pushPin = new VEPushPin(latLong);
            map.Items.Add(pushPin);
        }

        void MapLocationVM_TrackingObject(object sender, EventArgs e)
        {
            var latLong = new VELatLong(
               _TrackerViewModel.MapLocationVM.Latitude,
               _TrackerViewModel.MapLocationVM.Longitude);

            map.Items.Add(new VEPushPin(latLong));

            Thread.Sleep(100);

            Dispatcher.Invoke((Action)(() => map.FlyTo(
                latLong,
                -90,
                0,
                _TrackerViewModel.MapLocationVM.TargetAltitude,
                null)));
        }

        #region UI Event Handlers

        //private void map_MapLoaded(object sender, EventArgs e)
        //{
        //}

        //private void btnStyleRoad_Click(object sender, RoutedEventArgs e)
        //{
        //    map.MapStyle = InfoStrat.VE.VEMapStyle.Road;
        //}

        //private void btnStyleHybrid_Click(object sender, RoutedEventArgs e)
        //{
        //    map.MapStyle = InfoStrat.VE.VEMapStyle.Hybrid;
        //}

        //private void btnStyleAerial_Click(object sender, RoutedEventArgs e)
        //{
        //    map.MapStyle = InfoStrat.VE.VEMapStyle.Aerial;
        //}

        //private void btnUp_Click(object sender, RoutedEventArgs e)
        //{
        //    map.DoMapMove(0, 1000, false);
        //}

        //private void btnRight_Click(object sender, RoutedEventArgs e)
        //{
        //    map.DoMapMove(-1000, 0, false);
        //}

        //private void btnDown_Click(object sender, RoutedEventArgs e)
        //{
        //    map.DoMapMove(0, -1000, false);
        //}

        //private void btnLeft_Click(object sender, RoutedEventArgs e)
        //{
        //    map.DoMapMove(1000, 0, false);
        //}

        //private void btnZoomIn_Click(object sender, RoutedEventArgs e)
        //{
        //    map.DoMapZoom(1000, false);
        //}

        //private void btnZoomOut_Click(object sender, RoutedEventArgs e)
        //{
        //    map.DoMapZoom(-1000, false);
        //}

        //private void VEPushPin_Click(object sender, VEPushPinClickedEventArgs e)
        //{
        //    VEPushPin pin = sender as VEPushPin;

        //    map.FlyTo(pin.LatLong, -90, 0, 300, null);
        //}

        #endregion
    }
}
