using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using ThreeByte.Network;
using log4net;

namespace ThreeByte.Network.Devices
{
    public class LutronNWK : IDisposable, INotifyPropertyChanged
    {
        private static readonly ILog log = LogManager.GetLogger(typeof(LutronNWK));


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

        public LutronNWK(string ipAddress, int port) {
            _link = new AsyncNetworkLink(ipAddress, port);
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
                throw new ObjectDisposedException("LutronNWK");
            }
            _disposed = true;
            _link.DataReceived -= _link_DataReceived;
            _link.Dispose();
        }

        public void SetAreaScene(string area, int scene){
            //Action 6 - Set Scene number
            string message = string.Format("#AREA, {0}, 6, {1}\r\n", area, scene);
            byte[] data = Encoding.ASCII.GetBytes(message);
            _link.SendMessage(data);
        }

        public void SetDeviceButton(string device, int button) {
            //Action 6 - Set Scene number
            string message = string.Format("#DEVICE, {0}, {1}, 3\r\n", device, button);
            byte[] data = Encoding.ASCII.GetBytes(message);
            _link.SendMessage(data);
        }

    }
}
