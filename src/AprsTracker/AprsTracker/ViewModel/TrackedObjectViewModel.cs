using System.Collections.ObjectModel;
using APRS;


namespace AprsTracker.ViewModel
{
    public class TrackedObjectViewModel : BaseViewModel
    {
        private PacketInfoViewModel _LatestPacketInfoVM;

        public TrackedObjectViewModel(PacketInfo packetInfo)
        {
            PacketInfoHistory = new ObservableCollection<PacketInfoViewModel>();
            Update(packetInfo);
        }

        public PacketInfoViewModel LatestPacketInfoVM
        {
            get
            {
                return _LatestPacketInfoVM;
            }
            set
            {
                if (_LatestPacketInfoVM == value)
                    return;

                _LatestPacketInfoVM = value;
                
                OnPropertyChanged("LatestPacketInfoVM");
            }
        }

        public ObservableCollection<PacketInfoViewModel> PacketInfoHistory { get; private set; }

        public void Update(PacketInfo packetInfo)
        {
            var packetInfoVM = new PacketInfoViewModel(packetInfo);
            PacketInfoHistory.Add(packetInfoVM);
            LatestPacketInfoVM = packetInfoVM;
        }
    }
}
