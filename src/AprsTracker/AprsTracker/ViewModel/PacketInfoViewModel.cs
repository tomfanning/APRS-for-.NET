using System;
using APRS;

namespace AprsTracker.ViewModel
{
    public class PacketInfoViewModel : BaseViewModel
    {
        private PacketInfo _PacketInfo;

        public PacketInfoViewModel(PacketInfo packetInfo)
        {
            PacketInfo = packetInfo;
        }

        public PacketInfo PacketInfo
        {
            get
            {
                return _PacketInfo;
            }
            set
            {
                if (_PacketInfo == value)
                    return;
                _PacketInfo = value;
                OnPropertyChanged("PacketInfo");
                OnPropertyChanged("DateReceivedLocalTime");
            }
        }

        public DateTime DateReceivedLocalTime
        {
            get { return TimeZone.CurrentTimeZone.ToLocalTime(PacketInfo.ReceivedDate); }
        }
    }
}
