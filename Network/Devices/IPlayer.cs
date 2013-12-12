using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ThreeByte.Network.Devices {
    
    //Implements Preset control protocol for ColorKinetics iPlayer3
    public class IPlayer {

        private readonly AsyncNetworkLink _link;

        public IPlayer(string ipAddress, int port) {
            _link = new AsyncNetworkLink(ipAddress, port);
            _link.DataReceived += _link_DataReceived;
        }

        private void _link_DataReceived(object sender, EventArgs e) {
            // No-op
        }

        public void Preset(int preset) {
            string message = string.Format("X04{0:X2}", preset);
            _link.SendMessage(Encoding.ASCII.GetBytes(message));
        }

        public void Off() {
            string message = "X0100";
            _link.SendMessage(Encoding.ASCII.GetBytes(message));
        }

    }
}
