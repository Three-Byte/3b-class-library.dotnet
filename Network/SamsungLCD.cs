using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using ThreeByte.Network;
using log4net;
using ThreeByte.Network.Devices;

namespace ThreeByte.Network
{
    public class SamsungLCD : IDisposable, INotifyPropertyChanged
    {
        private static readonly ILog log = LogManager.GetLogger(typeof(SamsungLCD));

        private static readonly int TCP_PORT = 1515;

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
        private ComputerControl _networkStatus;

        public SamsungLCD(string ipAddress, string macAddress) {
            _link = new AsyncNetworkLink(ipAddress, TCP_PORT);
            _link.DataReceived += new EventHandler(_link_DataReceived);

            _networkStatus = new ComputerControl(ipAddress, macAddress);
            _networkStatus.PropertyChanged += _networkStatus_PropertyChanged;
        }

        void _networkStatus_PropertyChanged(object sender, PropertyChangedEventArgs e) {
            if(e.PropertyName == "Online") {
                NotifyPropertyChanged("IsPowerOn");
            }
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
                throw new ObjectDisposedException("SamsungLCD");
            }
            _disposed = true;
            _link.DataReceived -= _link_DataReceived;
            _link.Dispose();
        }

        public void Power(bool state) {
            if(state) {
                //Explicit messages don't actually work when the monitor is powered off
                //This isn't documented though
                //byte[] message = new byte[] { 0xAA, 0x11, 0xFE, 0x01, 0x01, 0x00 };
                //checksum(message);
                //_link.SendMessage(message);
                
                //Use WakeOnLan instead
                _networkStatus.Startup();
            } else {
                byte[] message = new byte[] { 0xAA, 0x11, 0xFE, 0x01, 0x00, 0x00 };
                checksum(message);
                _link.SendMessage(message);
            }
        }

        public bool IsPowerOn {
            get {
                return _networkStatus.Online;
            }
        }

        private void checksum(byte[] data) {

            byte sum = 0;
            for(int i = 1; i < data.Length - 1; i++) {
                sum += data[i];
            }
            data[data.Length - 1] = sum;
        }
       

    }
}
