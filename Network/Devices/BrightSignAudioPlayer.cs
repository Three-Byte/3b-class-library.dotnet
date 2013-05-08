using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ThreeByte.Network;

namespace ThreeByte.Network.Devices
{
    public class BrightSignAudioPlayer
    {

        private AsyncUdpLink _networkLink;

        public BrightSignAudioPlayer(string address, int port) {
            _networkLink = new AsyncUdpLink(address, port);
        }

        public void PlayFile(string file) {
            byte[] data = Encoding.ASCII.GetBytes(file);
            _networkLink.SendMessage(data);
        }

        public void Stop() {
            byte[] data = Encoding.ASCII.GetBytes("stop");
            _networkLink.SendMessage(data);
        }
    }
}
