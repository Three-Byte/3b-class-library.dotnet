using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using ThreeByte.Network;

namespace ThreeByte.Network
{
    public class PowerStrip : IDisposable, INotifyPropertyChanged
    {
        //Telnet: port is 23
        private static readonly int TCP_PORT = 23;

        #region Public Properties
        //Observable Interface
        public event PropertyChangedEventHandler PropertyChanged;

        private void NotifyPropertyChanged(String info) {
            if(PropertyChanged != null) {
                PropertyChanged(this, new PropertyChangedEventArgs(info));
            }
        }

        #endregion Public Properties

        private AsyncNetworkLink _link;

        public PowerStrip(string ipAddress) {
            _link = new AsyncNetworkLink(ipAddress, TCP_PORT);
        }

        private bool _disposed = false;
        public void Dispose() {
            if(_disposed) {
                throw new ObjectDisposedException("PowerStrip");
            }
            _disposed = true;
            _link.Dispose();
        }

        public void Power(int outlet, bool state) {
            string message = string.Format("pset {0} {1}\r\n", outlet, (state ? "1" : "0"));
            _link.SendMessage(Encoding.ASCII.GetBytes(message));
        }
        

    }
}
