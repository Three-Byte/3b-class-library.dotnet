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
        
        public string IpAddress {
            get {
                return _networkStatus.Host;
            }
        }

        public string MacAddress {
            get {
                return _networkStatus.MacAddress;
            }
        }

        //A value between 0-100
        private int _backlight;
        public int Backlight {
            get {
                return _backlight;
            }
            set {
                _backlight = value;
                NotifyPropertyChanged("Backlight");
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

        private void _networkStatus_PropertyChanged(object sender, PropertyChangedEventArgs e) {
            if(e.PropertyName == "Online") {
                NotifyPropertyChanged("IsPowerOn");
            }
        }

        private void _link_DataReceived(object sender, EventArgs e) {
            while(_link.HasData) {
                byte[] data = _link.GetMessage();
                log.InfoFormat("Data Received: {0}", printBytes(data));
                ParseReceivedData(data);
            }
        }

        private void ParseReceivedData(byte[] data) {

            //See if this is a Backlight status message
            if(data.Length == 8) {

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

        /// <summary>
        /// Set the input source for the monitor
        /// </summary>
        /// <param name="input">1:PC 2:BNC 3:DVI 4:AV 5:S-Video 6:Component 7:MagicNet</param>
        public void Input(int input) {
            //Set the input between HDMI/VGA

            byte inputVal = 0;
            switch(input) {
                case 1:
                    inputVal = 0x14; //PC
                    break;
                case 2:
                    inputVal = 0x1E; //BNC
                    break;
                case 3:
                    inputVal = 0x18; //DVI
                    break;
                case 4:
                    inputVal = 0x0C; //AV
                    break;
                case 5:
                    inputVal = 0x04; //S-Video
                    break;
                case 6:
                    inputVal = 0x08; //Component
                    break;
                case 7:
                    inputVal = 0x20; //MagicNet
                    break;
                default:
                    throw new ArgumentOutOfRangeException("input");
            }

            byte[] message = new byte[] { 0xAA, 0x14, 0xFE, 0x01, inputVal, 0x00 };
            checksum(message);
            _link.SendMessage(message);
        }


        //TODO: make this private
        public void QueryBacklight() {
            byte[] message = new byte[] { 0xAA, 0x58, 0xFE, 0x00, 0x00 };
            checksum(message);
            _link.SendMessage(message);
        }

        public void QueryStatus() {
            byte[] message = new byte[] { 0xAA, 0x00, 0xFE, 0x00, 0x00 };
            checksum(message);
            _link.SendMessage(message);
        }

        /// <summary>
        /// Sets the backlight lamp value
        /// </summary>
        /// <param name="value">Lamp value (0 - 100)</param>
        public void SetBacklight(int value) {
            byte[] message = new byte[] { 0xAA, 0x58, 0xFE, 0x01, (byte)value, 0x00 };
            checksum(message);
            _link.SendMessage(message);
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
