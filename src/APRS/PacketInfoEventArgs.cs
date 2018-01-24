using System;

namespace APRS
{
    public class PacketInfoEventArgs : EventArgs
    {
        public PacketInfoEventArgs(PacketInfo pi)
        {
            PacketInfo = pi;
        }

        public PacketInfo PacketInfo { get; private set; }
    }
}