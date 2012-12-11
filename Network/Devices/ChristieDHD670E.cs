using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using ThreeByte.Network;
using log4net;

namespace ThreeByte.Network.Devices
{
    public class ChristieDHD670E : IDisposable, INotifyPropertyChanged
    {
        private static readonly ILog log = LogManager.GetLogger(typeof(ChristieDHD670E));

        //Default port is 3002
        private static readonly int TCP_PORT = 3002;

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

        public ChristieDHD670E(string ipAddress) {
            _link = new AsyncNetworkLink(ipAddress, TCP_PORT);
            _link.DataReceived += new EventHandler(_link_DataReceived);
        }

        void _link_DataReceived(object sender, EventArgs e) {
            while(_link.HasData) {
                byte[] data = _link.GetMessage();
                log.InfoFormat("Data Received: {0}", printBytes(data));
            }
        }

        private string printBytes(byte[] data) {
            StringBuilder sb = new StringBuilder();
            foreach(byte b in data) {
                sb.AppendFormat("{0:X2},", b);
            }
            return sb.ToString();
        }

        private bool _disposed = false;
        public void Dispose() {
            if(_disposed) {
                throw new ObjectDisposedException("ChristieDHD670E");
            }
            _disposed = true;
            _link.DataReceived -= _link_DataReceived;
            _link.Dispose();
        }

        public void Power(bool state){
            string message = string.Empty;
            if(state) {
                message = "(PWR1)";
            } else {
                message = "(PWR0)";
            }
            byte[] data = Encoding.ASCII.GetBytes(message);
            _link.SendMessage(data);
        }

    }
}
