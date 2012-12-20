using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using ThreeByte.Network;
using log4net;

namespace ThreeByte.Network.Devices
{
    public class SonyVisca : IDisposable, INotifyPropertyChanged
    {
        private static readonly ILog log = LogManager.GetLogger(typeof(ExtronMAV));

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

        public SonyVisca(string ipAddress, int port) {
            _link = new AsyncNetworkLink(ipAddress, port);
            _link.DataReceived += new EventHandler(_link_DataReceived);
        }

        void _link_DataReceived(object sender, EventArgs e) {
            while(_link.HasData) {
                byte[] data = _link.GetMessage();
                log.InfoFormat("Data Received: {0}", Encoding.ASCII.GetString(data));
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
                throw new ObjectDisposedException("SonyVisca");
            }
            _disposed = true;
            _link.DataReceived -= _link_DataReceived;
            _link.Dispose();
        }

        public void RecallPreset(int preset) {
            byte[] data = new byte[] { 0x81, 0x01, 0x04, 0x3F, 0x02, (byte)(preset - 1), 0xFF };
            _link.SendMessage(data);

        }
       
    }
}
